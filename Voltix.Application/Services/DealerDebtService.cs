using Aspose.Words;
using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.DealerDebt;
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
    public class DealerDebtService : IDealerDebtService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDealerDebtTransactionService _dealerDebtTransactionService;
        private readonly IMapper _mapper;
        public DealerDebtService(IUnitOfWork unitOfWork, IDealerDebtTransactionService dealerDebtTransactionService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _dealerDebtTransactionService = dealerDebtTransactionService;
            _mapper = mapper;
        }

        public async Task<ResponseDTO> AddPurchaseForDealerAsync(Guid dealerId, RecordDebtDTO debtDTO, CancellationToken ct)
        {
            try
            {
                var now = DateTime.SpecifyKind(debtDTO.ConfirmDateUtc, DateTimeKind.Utc);

                var create = new CreateDealerDebtTransactionDTO
                {
                    DealerId = dealerId,
                    Type = DealerDebtTransactionType.Purchase,
                    Amount = debtDTO.Amount,
                    OccurredAtUtc = now,
                    ExternalId = BuildExtId("PURCHASE", debtDTO.ReferenceNo, dealerId, now),
                    SourceType = debtDTO.SourceType,
                    SourceId = debtDTO.SourceId,
                    SourceNo = debtDTO.ReferenceNo,
                    ReferenceNo = debtDTO.ReferenceNo,
                    Note = debtDTO.Note
                };

                await _dealerDebtTransactionService.CraeteDealerDebtTransaction(create, ct);
                await _unitOfWork.SaveAsync();
                return new ResponseDTO
                {
                    StatusCode = 201,
                    IsSuccess = true,
                    Message = "Successfully added purchase for dealer debt.",
                    Result = create
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = $"Failed to add purchase for dealer debt: {ex.Message}"
                };
            }
        }

        private static string BuildExtId(string kind, string? referenceNo, Guid dealerId, DateTime now)
        {
            if (!string.IsNullOrWhiteSpace(referenceNo))
                return $"{kind}:{referenceNo}".Trim();

            return $"{kind}:{dealerId}:{now:yyyyMMddHHmmss}";
        }

        public async Task<ResponseDTO> AddPaymentForDealerAsync(Guid dealerId, RecordPaymentDTO paymentDTO, CancellationToken ct)
        {
            try
            {
                var now = DateTime.SpecifyKind(paymentDTO.PaidAtUtc, DateTimeKind.Utc);

                var create = new CreateDealerDebtTransactionDTO
                {
                    DealerId = dealerId,
                    Type = DealerDebtTransactionType.Payment,
                    Amount = paymentDTO.Amount,
                    OccurredAtUtc = now,
                    ExternalId = BuildExtId("PAYMENT", paymentDTO.ReferenceNo, dealerId, now),
                    Method = paymentDTO.Method,
                    SourceType = paymentDTO.SourceType,
                    SourceId = paymentDTO.SourceId,
                    SourceNo = paymentDTO.ReferenceNo,
                    ReferenceNo = paymentDTO.ReferenceNo,
                    Note = paymentDTO.Note
                };

                await _dealerDebtTransactionService.CraeteDealerDebtTransaction(create, ct);

                return new ResponseDTO()
                {
                    StatusCode = 200,
                    Message = "Successfully recorded payment for dealer debt."
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO()
                {
                    StatusCode = 500,
                    Message = $"Failed to record payment: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> AddCommissionForDealerAsync(Guid dealerId, RecordCommissionDTO dto, CancellationToken ct)
        {
            try
            {
                var now = DateTime.SpecifyKind(dto.AtUtc, DateTimeKind.Utc);

                var create = new CreateDealerDebtTransactionDTO
                {
                    DealerId = dealerId,
                    Type = DealerDebtTransactionType.Commission,
                    Amount = dto.Amount,
                    OccurredAtUtc = now,
                    ExternalId = BuildExtId("COMMISSION", dto.ReferenceNo, dealerId, now),
                    SourceType = dto.SourceType,
                    SourceId = dto.SourceId,
                    SourceNo = dto.ReferenceNo,
                    ReferenceNo = dto.ReferenceNo,
                    Note = dto.Note
                };

                await _dealerDebtTransactionService.CraeteDealerDebtTransaction(create, ct);
                await _unitOfWork.SaveAsync();
                return new ResponseDTO
                {
                    StatusCode = 200,
                    Message = "Successfully recorded commission for dealer."
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO()
                {
                    StatusCode = 500,
                    Message = $"Failed to record commission: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> GetDealerDebtBalanceAtQuarterNow(ClaimsPrincipal userClaim, Guid? dealerId, CancellationToken ct)
        {
            try
            {
                var userId = userClaim.FindFirst(ClaimTypes.NameIdentifier)!.Value;
                if (userId is null)
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = false,
                        StatusCode = 401,
                        Message = "The user not login yet."
                    };
                }

                var role = userClaim.FindFirst(ClaimTypes.Role)!.Value;
                if (role is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 403,
                        Message = "The user has no role assigned."
                    };
                }

                Dealer? dealer;
                if (role.Equals(StaticUserRole.Admin) || role.Equals(StaticUserRole.EVMStaff))
                {
                    if (dealerId is null)
                    {
                        return new ResponseDTO(false)
                        {
                            StatusCode = 400,
                            Message = "DealerId is required for admin or staff users."
                        };
                    }

                    dealer = await _unitOfWork.DealerRepository.GetByIdAsync(dealerId.Value, ct);
                    if (dealer is null)
                    {
                        return new ResponseDTO(false)
                        {
                            StatusCode = 404,
                            Message = "Dealer not found."
                        };
                    }
                }
                else
                {
                    dealer = await _unitOfWork.DealerRepository.GetDealerByManagerIdAsync(userId, ct);
                    if (dealer is null)
                    {
                        dealer = await _unitOfWork.DealerRepository.GetDealerByUserIdAsync(userId, ct);
                        if (dealer is null)
                        {
                            return new ResponseDTO(false)
                            {
                                StatusCode = 404,
                                Message = "Dealer not found for the current user."
                            };
                        }
                    }
                }

                var dealerDebt = await _unitOfWork.DealerDebtRepository.GetOrCreateQuarterAsync(dealer.Id, DateTime.Now, ct);
                if (dealerDebt is null)
                {
                    return new ResponseDTO(false)
                    {
                        StatusCode = 404,
                        Message = "Dealer debt record not found."
                    };
                }

                var getDealerDebt = _mapper.Map<GetDealerDebtDTO>(dealerDebt);
                return new ResponseDTO
                {
                    StatusCode = 200,
                    Message = "Successfully retrieved dealer debt balance.",
                    Result = getDealerDebt
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO(false)
                {
                    StatusCode = 500,
                    Message = $"Failed to retrieve dealer debt balance: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> GetDealerDebtDetails(ClaimsPrincipal userClaim, Guid? dealerId, DateTime fromDateUtc, DateTime toDateUtc, int pageNumber, int pageSize, CancellationToken ct)
        {
            try
            {
                var userId = userClaim.FindFirst(ClaimTypes.NameIdentifier)!.Value;
                if (userId is null)
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = false,
                        StatusCode = 401,
                        Message = "The user not login yet."
                    };
                }

                var role = userClaim.FindFirst(ClaimTypes.Role)!.Value;
                if (role is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 403,
                        Message = "The user has no role assigned."
                    };
                }

                Dealer? dealer;
                if (role.Equals(StaticUserRole.Admin) || role.Equals(StaticUserRole.EVMStaff))
                {
                    if (dealerId is null)
                    {
                        return new ResponseDTO(false)
                        {
                            StatusCode = 400,
                            Message = "DealerId is required for admin or staff users."
                        };
                    }

                    dealer = await _unitOfWork.DealerRepository.GetByIdAsync(dealerId.Value, ct);
                    if (dealer is null)
                    {
                        return new ResponseDTO(false)
                        {
                            StatusCode = 404,
                            Message = "Dealer not found."
                        };
                    }
                }
                else
                {
                    dealer = await _unitOfWork.DealerRepository.GetDealerByManagerIdAsync(userId, ct);
                    if (dealer is null)
                    {
                        dealer = await _unitOfWork.DealerRepository.GetDealerByUserIdAsync(userId, ct);
                        if (dealer is null)
                        {
                            return new ResponseDTO(false)
                            {
                                StatusCode = 404,
                                Message = "Dealer not found for the current user."
                            };
                        }
                    }
                }

                Expression<Func<DealerDebtTransaction, bool>> filter = ddt => ddt
                    .DealerId == dealer.Id &&
                    ddt.CreatedAtUtc >= fromDateUtc &&
                    ddt.CreatedAtUtc <= toDateUtc;

                (IReadOnlyList<DealerDebtTransaction> items, int total) result;

                result = await _unitOfWork.DealerDebtTransactionRepository.GetPagedAsync(
                            filter: filter,
                            includes: null,
                            orderBy: dm => dm.CreatedAtUtc,
                            ascending: false,
                            pageNumber: pageNumber,
                            pageSize: pageSize,
                            ct: ct);

                var dealerDebtDetails = _mapper.Map<List<GetDealerDebtTransactionDTO>>(result.items);
                return new ResponseDTO
                {
                    StatusCode = 200,
                    Message = "Successfully retrieved dealer debt details.",
                    Result = new
                    {
                        data = dealerDebtDetails,
                        Pagination = new
                        {
                            PageNumber = pageNumber,
                            PageSize = pageSize,
                            TotalItems = result.total,
                            TotalPages = (int)Math.Ceiling((double)result.total / pageSize)
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO(false)
                {
                    StatusCode = 500,
                    Message = $"Failed to retrieve dealer debt details: {ex.Message}"
                };
            }
        }
    }
}
