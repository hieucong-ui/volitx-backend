using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.Quote;
using Voltix.Application.IServices;
using Voltix.Domain.Constants;
using Voltix.Domain.Entities;
using Voltix.Domain.Enums;
using Voltix.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.Services
{
    public class QuoteService : IQuoteService
    {
        public readonly IUnitOfWork _unitOfWork;
        public readonly IMapper _mapper;
        public readonly ILogService _logService;
        public QuoteService(IUnitOfWork unitOfWork, IMapper mapper, ILogService logService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logService = logService;
        }
        public async Task<ResponseDTO> CreateQuoteAsync(ClaimsPrincipal user, CreateQuoteDTO createQuoteDTO)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User not found",
                        StatusCode = 404
                    };
                }

                var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerOrStaffAsync(userId, CancellationToken.None);
                if(dealer == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Dealer not found",
                        StatusCode = 404
                    };
                }

                var warehouse = await _unitOfWork.WarehouseRepository.GetWarehouseByDealerIdAsync(dealer.Id);
                if (warehouse == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Warehouse 's dealer is not found ",
                        StatusCode = 404
                    };
                }

                Quote quote = new Quote
                {
                    DealerId = dealer.Id,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow,
                    Status = QuoteStatus.Pending,
                    Note = createQuoteDTO.Note,
                    QuoteDetails = new List<QuoteDetail>()
                };

                decimal totalAmount = 0;

                foreach( var dt in createQuoteDTO.QuoteDetails)
                {
                    // take version
                    var version = await _unitOfWork.ElectricVehicleVersionRepository.GetByIdsAsync(dt.VersionId);
                    if (version == null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = " No version found ",
                            StatusCode = 404
                        };
                    }

                    //take color
                    var color = await _unitOfWork.ElectricVehicleColorRepository.GetByIdsAsync(dt.ColorId);
                    if(color == null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = " No color found ",
                            StatusCode = 404
                        };
                    }

                    var availableVehicles = await _unitOfWork.ElectricVehicleRepository
                        .GetAvailableVehicleByDealerAsync(dealer.Id,version.Id,color.Id);
                    if (availableVehicles == null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "No available vehicle in dealer 's warehouse",
                            StatusCode = 404
                        };
                    }

                    var templates = (await _unitOfWork.EVTemplateRepository.GetTemplatesByVersionAndColorAsync(version.Id,color.Id));
                    if (templates == null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "No templates found",
                            StatusCode = 404
                        };
                    }

                    decimal basePrice = templates.Price;
                    decimal extraPrice = color.ExtraCost;
                    decimal unitPrice = basePrice + extraPrice;
                    decimal discount = 0;
                    Promotion? promotion = null;

                    if (dt.PromotionId != null)
                    {
                        promotion = await _unitOfWork.PromotionRepository.GetPromotionByIdAsync(dt.PromotionId.Value);
                        if(promotion == null)
                        {
                            return new ResponseDTO
                            {
                                IsSuccess = false,
                                Message = "Promotion not found",
                                StatusCode = 404,
                            };
                        }

                        if (!promotion.IsActive || promotion.StartDate > DateTime.UtcNow || promotion.EndDate < DateTime.UtcNow)
                        {
                            return new ResponseDTO
                            {
                                IsSuccess = false,
                                Message = "Promotion not active",
                                StatusCode = 400
                            };
                        }

                        if ((promotion.ModelId == null && promotion.VersionId == null)
                            || (promotion.ModelId == version.ModelId && promotion.VersionId == version.Id))
                        {
                            if (promotion.DiscountType == DiscountType.Percentage && promotion.Percentage.HasValue)
                            {
                                discount = (unitPrice * promotion.Percentage.Value) / 100;
                            }
                            else if (promotion.DiscountType == DiscountType.FixAmount && promotion.FixedAmount.HasValue)
                            {
                                discount = promotion.FixedAmount.Value;
                            }
                        }
                    }

                    decimal totalPrice = (unitPrice - discount) * dt.Quantity;
                    totalPrice = Math.Ceiling(totalPrice);
                    totalAmount += totalPrice;

                    var quoteDetail = new QuoteDetail
                    {
                        VersionId = version.Id,
                        ColorId = color.Id,
                        Quantity = dt.Quantity,
                        PromotionId = dt.PromotionId,
                        UnitPrice = unitPrice,
                        Promotion = promotion,
                        TotalPrice = totalPrice,
                    };
                    quote.QuoteDetails.Add(quoteDetail);
                }
                quote.TotalAmount = totalAmount;

                await _unitOfWork.QuoteRepository.AddAsync(quote, CancellationToken.None);
                await _unitOfWork.SaveAsync();
                await _logService.AddLogAsync(user, LogType.Create, "Quote", quote.Note, CancellationToken.None);

                var quoteDTO = await _unitOfWork.QuoteRepository.GetQuoteByIdAsync(quote.Id);
                var getQuoteDTO = _mapper.Map<GetQuoteDTO>(quoteDTO);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Create quote successfully",
                    StatusCode = 200,
                    Result = getQuoteDTO
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500,
                };
            }

            }

        public async Task<ResponseDTO> GetAllAsync(ClaimsPrincipal user, int pageNumber , int pageSize , Guid? modelId,Guid? versionId,Guid? colorId, QuoteStatus? status, bool onlyToday = false, CancellationToken ct = default)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User not found",
                        StatusCode = 404
                    };
                }

                var role = user.FindFirst(ClaimTypes.Role)?.Value;

                Expression<Func<Quote, bool>>? filter = null;
                if (status.HasValue)
                    filter = q => q.Status == status.Value;

                Guid? dealerId = null;

                if (role == StaticUserRole.DealerManager || role == StaticUserRole.DealerStaff)
                {
                    var dealer = await _unitOfWork.DealerRepository
                        .GetDealerByManagerOrStaffAsync(userId, ct);
                    if (dealer == null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Dealer not found",
                            StatusCode = 404
                        };
                    }
                    dealerId = dealer.Id;
                }

                var query = _unitOfWork.QuoteRepository.Query();
                if (status.HasValue)
                {
                    query = query.Where(q => q.Status == status.Value);
                }

                if (dealerId.HasValue)
                {
                    query = query.Where(q => q.DealerId == dealerId.Value);
                }

                if (onlyToday)
                {
                    var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                    var nowVN = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
                    var todayVN = nowVN.Date;
                    var tomorrowVN = todayVN.AddDays(1);

                    var todayUTC = TimeZoneInfo.ConvertTimeToUtc(todayVN, vnTimeZone);
                    var tomorrowUTC = TimeZoneInfo.ConvertTimeToUtc(tomorrowVN, vnTimeZone);

                    query = query.Where(q => q.CreatedAt >= todayUTC && q.CreatedAt < tomorrowUTC);
                }

                if (modelId.HasValue || versionId.HasValue || colorId.HasValue)
                {
                    query = query.Where(q => q.QuoteDetails.Any(dt =>
                        (!modelId.HasValue || dt.ElectricVehicleVersion.ModelId == modelId.Value) &&
                        (!versionId.HasValue || dt.VersionId == versionId.Value) &&
                        (!colorId.HasValue || dt.ColorId == colorId.Value)
                    ));
                }

                query = query
                    .Include(x => x.QuoteDetails)
                        .ThenInclude(d => d.Promotion)
                    .Include(x => x.QuoteDetails)
                        .ThenInclude(d => d.ElectricVehicleVersion)
                            .ThenInclude(v => v.Model)
                    .Include(x => x.QuoteDetails)
                        .ThenInclude(d => d.ElectricVehicleColor)
                    .Include(x => x.Dealer);

                var totalItems = await query.CountAsync(ct);
                var quotes = await query
                    .OrderByDescending(q => q.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(ct);

                if (!quotes.Any())
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "No quotes found for specified criteria",
                        StatusCode = 404
                    };
                }

                var filteredQuotes = new List<GetQuoteDTO>();

                foreach (var q in quotes)
                {
                    bool isShow = true;

                    foreach (var dt in q.QuoteDetails)
                    {
                        // Check promotion expiration
                        if (dt.Promotion != null &&
                            dt.Promotion.EndDate.HasValue &&
                            dt.Promotion.EndDate < DateTime.UtcNow)
                        {
                            isShow = false;
                            break;
                        }

                        // Check vehicle availability
                        var availableVehicles = await _unitOfWork.ElectricVehicleRepository
                            .GetAvailableVehicleByDealerAsync(q.DealerId, dt.VersionId, dt.ColorId);

                        if (!availableVehicles.Any())
                        {
                            isShow = false;
                            break;
                        }
                    }

                    if (isShow)
                    {
                        filteredQuotes.Add(_mapper.Map<GetQuoteDTO>(q));
                    }
                }

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Get all quotes successfully",
                    StatusCode = 200,
                    Result = new
                    {
                        data = filteredQuotes,
                        Pagination = new
                        {
                            PageNumber = pageNumber,
                            PageSize = pageSize,
                            TotalItems = totalItems,
                            TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> GetQuoteByIdAsync(ClaimsPrincipal user,  Guid id)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if(userId == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User not found",
                        StatusCode = 404
                    };
                }

                var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerOrStaffAsync(userId,CancellationToken.None);
                if (dealer == null)
                {
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Dealer not found",
                            StatusCode = 404
                        };
                    }
                }
                var quote = await _unitOfWork.QuoteRepository.GetQuoteByIdAsync(id);
                if (quote == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Quote not found",
                        StatusCode = 404,
                    };
                }

                foreach (var dt in quote.QuoteDetails)
                {
                    var availableVehicles = await _unitOfWork.ElectricVehicleRepository
                        .GetAvailableVehicleByDealerAsync(quote.DealerId, dt.VersionId, dt.ColorId);

                    if (availableVehicles.Count() < dt.Quantity)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Not enough vehicles in warehouse for this quote",
                            StatusCode = 404
                        };
                    }
                }

                var getQuote = _mapper.Map<GetQuoteDTO>(quote);
                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Quote retrieved successfully",
                    StatusCode = 200,
                    Result = getQuote
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500,
                };

            }
        }

        public async Task<ResponseDTO> UpdateExpiredQuoteAsync(ClaimsPrincipal user, CancellationToken ct)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User not found",
                        StatusCode = 400
                    };
                }

                var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerIdAsync(userId, ct);
                if(dealer == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Dealer not found",
                        StatusCode = 404
                    };
                }

                var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var todayVN = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone).Date;
                var todayStartUTC = TimeZoneInfo.ConvertTimeToUtc(todayVN, vnTimeZone);

                // Take all quote that status != Expired
                var quotes = await _unitOfWork.QuoteRepository.Query(
                    filter: q => q.DealerId == dealer.Id && q.Status != QuoteStatus.Expired && q.CreatedAt < todayStartUTC,
                    includes: null,
                    asNoTracking: false  // quan trọng: cho EF track entity
                ).ToListAsync(ct);

                if (!quotes.Any())
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "No quotes to update",
                        StatusCode = 400
                    };
                }

                foreach (var quote in quotes)
                {
                    quote.Status = QuoteStatus.Expired;
                }

                await _unitOfWork.SaveAsync(ct);
                await _logService.AddLogAsync(user, LogType.Update, "Quote", null, ct);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Updated successfully",
                    StatusCode = 200,
                };

            }
            catch(Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500,
                };
            }
        }

        public async Task<ResponseDTO> UpdateQuoteStatusAsync(ClaimsPrincipal user, Guid id, QuoteStatus newStatus)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if(userId == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User not found",
                        StatusCode = 404
                    };
                }

                var dealer = await _unitOfWork.DealerRepository.GetManagerByUserIdAsync(userId, CancellationToken.None);
                if(dealer == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Dealer not found",
                        StatusCode = 403
                    };
                }

                var quote = await _unitOfWork.QuoteRepository.GetQuoteByIdAsync(id);
                if (quote == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Quote not found",
                        StatusCode = 404,
                    };
                }

                if(quote.Status == QuoteStatus.Accepted
                    || quote.Status == QuoteStatus.Rejected)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Cannot update status of a approved or rejected quote",
                        StatusCode = 404
                    };
                }

                //Check logic before change status
                if(newStatus == QuoteStatus.Pending)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = " Cann't update to status pending",
                        StatusCode = 400
                    };
                }

                if(newStatus == QuoteStatus.Accepted)
                {
                    var warehouse = await _unitOfWork.WarehouseRepository.GetWarehouseByDealerIdAsync(quote.DealerId);
                    if(warehouse == null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Dealer's Warehouse not found",
                            StatusCode = 404
                        };
                    }

                    foreach(var dt in quote.QuoteDetails)
                    {
                        //Take Vehicle (Status = AtDealer)
                        var availableVehicles = await _unitOfWork.ElectricVehicleRepository
                            .GetAvailableVehicleByDealerAsync(dt.Quote.DealerId, dt.VersionId, dt.ColorId);
                        if (availableVehicles.Count() < dt.Quantity)
                        {
                            return new ResponseDTO
                            {
                                IsSuccess = false,
                                Message = "Not enough vehicle in dealer 's warehouse",
                                StatusCode = 404,
                            };
                        }

                        // Take ev with ImportDate oldest
                        var selectedVehicle = availableVehicles
                            .OrderBy(ev => ev.ImportDate)
                            .Take(dt.Quantity)
                            .ToList();
                    }
                }

                quote.Status = newStatus;
                _unitOfWork.QuoteRepository.Update(quote);
                await _unitOfWork.SaveAsync();
                await _logService.AddLogAsync(user, LogType.Update, "Quote", quote.Note, CancellationToken.None);

                string message = newStatus switch
                {
                    QuoteStatus.Accepted => "Booking approved successfully",
                    QuoteStatus.Rejected => "Booking rejected successfully",
                    _ => "Booking status updated successfully"
                };
                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Quote retrieved successfully",
                    StatusCode = 200,
                };

            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500,
                };
            }

        }
    }
}
