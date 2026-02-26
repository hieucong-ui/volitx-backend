using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.BookingEV;
using Voltix.Application.DTO.BookingEVDetail;
using Voltix.Application.DTO.VehicleDelivery;
using Voltix.Application.DTO.Dealer;
using Voltix.Application.DTO.DealerDebt;
using Voltix.Application.IServices;
using Voltix.Domain.Constants;
using Voltix.Domain.Entities;
using Voltix.Domain.Enums;
using Voltix.Infrastructure.IRepository;
using Voltix.Infrastructure.SignlR;
using System.Linq.Expressions;
using System.Security.Claims;
using Voltix.Application.DTO.VehicleDeliveryDetail;
using StackExchange.Redis;

namespace Voltix.Application.Services
{
    public class BookingEVService : IBookingEVService
    {
        public readonly IUnitOfWork _unitOfWork;
        public readonly IMapper _mapper;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IDealerDebtService _dealerDebt;
        private readonly IEContractService _eContractService;
        private readonly ILogService _logService;

        public BookingEVService(IUnitOfWork unitOfWork, IMapper mapper, IHubContext<NotificationHub> hubContext, IDealerDebtService dealerDebt, IEContractService eContractService, ILogService logService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            _dealerDebt = dealerDebt ?? throw new ArgumentNullException(nameof(dealerDebt));
            _eContractService = eContractService ?? throw new ArgumentNullException(nameof(eContractService));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));

        }

        public async Task<ResponseDTO> ConfirmBookingDeliveryAsync(ClaimsPrincipal user, Guid bookingId, CancellationToken ct)
        {
            try
            {
                var bookingEV = await _unitOfWork.BookingEVRepository.GetBookingWithIdAsync(bookingId);
                if (bookingEV is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Booking not found.",
                        StatusCode = 404
                    };
                }

                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User not login yet.",
                        StatusCode = 401
                    };
                }

                var dealer = await _unitOfWork.DealerRepository.GetManagerByUserIdAsync(userId, ct);
                if (dealer is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Dealer not found.",
                        StatusCode = 404
                    };
                }

                if (bookingEV.Dealer.Id != dealer.Id)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Unauthorized to confirm this booking.",
                        StatusCode = 403
                    };
                }

                if (bookingEV.Status != BookingStatus.SignedByAdmin)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Can only complete an signedByAdmin booking.",
                        StatusCode = 400
                    };
                }

                var warehouse = await _unitOfWork.WarehouseRepository.GetWarehouseByDealerIdAsync(bookingEV.DealerId);
                if (warehouse == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Dealer's warehouse not found.",
                        StatusCode = 404
                    };
                }

                var vehicleDelivery = await _unitOfWork.VehicleDeliveryRepository.GetVehicleDeliveryByBookingId(bookingEV.Id, ct);
                if (vehicleDelivery == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Delivery not found",
                        StatusCode = 400
                    };
                }

                foreach (var dt in vehicleDelivery.VehicleDeliveryDetails)
                {
                    if (dt.ElectricVehicle != null)
                    {
                        dt.ElectricVehicle.Status = ElectricVehicleStatus.AtDealer;
                        dt.ElectricVehicle.WarehouseId = warehouse.Id;
                        _unitOfWork.ElectricVehicleRepository.Update(dt.ElectricVehicle);

                        dt.Status = DeliveryVehicleStatus.Delivered;
                        _unitOfWork.VehicleDeliveryDetailRepository.Update(dt);
                    }
                }

                vehicleDelivery.Status = DeliveryStatus.Confirmed;
                vehicleDelivery.UpdateAt = DateTime.UtcNow;
                _unitOfWork.VehicleDeliveryRepository.Update(vehicleDelivery);

                var amount = await CalPriceBookingEV(bookingEV);
                var newPurchase = new RecordDebtDTO
                {
                    ReferenceNo = $"Booking_{bookingEV.Id}",
                    Amount = amount,
                    ConfirmDateUtc = DateTime.UtcNow
                };
                await _dealerDebt.AddPurchaseForDealerAsync(dealer.Id, newPurchase, ct);

                bookingEV.Status = BookingStatus.Completed;
                _unitOfWork.BookingEVRepository.Update(bookingEV);
                await _unitOfWork.SaveAsync();
                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Booking delivery confirmed successfully.",
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Fail to confirm booking: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        private async Task<decimal> CalPriceBookingEV(BookingEV bookingEV)
        {
            decimal amount = 0;
            foreach (var detail in bookingEV.BookingEVDetails)
            {
                var vehicle = await _unitOfWork.EVTemplateRepository.GetTemplatesByVersionAndColorAsync(detail.VersionId, detail.ColorId);
                if (vehicle != null)
                {
                    amount += vehicle.Price * detail.Quantity;
                }
            }

            return amount;
        }

        public async Task<ResponseDTO> CreateBookingEVAsync(ClaimsPrincipal user, CreateBookingEVDTO createBookingEVDTO, CancellationToken ct)
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
                        StatusCode = 404,
                    };
                }

                var dealer = await _unitOfWork.DealerRepository.GetManagerByUserIdAsync(userId, ct);

                if (dealer == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Dealer not found",
                        StatusCode = 404,
                    };
                }

                var bookingEV = new BookingEV
                {
                    DealerId = dealer.Id,
                    Note = createBookingEVDTO.Note,
                    BookingDate = DateTime.UtcNow,
                    Status = BookingStatus.WaitingDealerSign,
                    CreatedBy = dealer.Name,
                    TotalQuantity = createBookingEVDTO.BookingDetails.Sum(d => d.Quantity),
                };
                bookingEV.BookingEVDetails = createBookingEVDTO.BookingDetails.Select(detail => new BookingEVDetail
                {
                    VersionId = detail.VersionId,
                    ColorId = detail.ColorId,
                    Quantity = detail.Quantity,
                }).ToList();
                // change status to pending
                foreach (var dt in bookingEV.BookingEVDetails)
                {
                    var version = await _unitOfWork.ElectricVehicleVersionRepository.GetByIdsAsync(dt.VersionId);
                    if (version == null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = " Version not found ",
                            StatusCode = 404
                        };
                    }

                    var availableVehicles = (await _unitOfWork.ElectricVehicleRepository
                        .GetAvailableVehicleByModelVersionColorAsync(version.ModelId, dt.VersionId, dt.ColorId))
                        .Where(ev => ev.Warehouse.WarehouseType == WarehouseType.EVInventory)
                        .OrderBy(ev => ev.ImportDate)
                        .ToList();

                    if (availableVehicles == null || !availableVehicles.Any())
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = " No available vehicles in EVM warehouse",
                            StatusCode = 404
                        };
                    }

                    if (availableVehicles.Count < dt.Quantity)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = " Not enough vehicles for booking",
                            StatusCode = 400
                        };
                    }

                    var selectedVehicles = availableVehicles
                        .OrderBy(ev => ev.ImportDate)
                        .Take(dt.Quantity)
                        .ToList();

                    foreach (var ev in selectedVehicles)
                    {
                        ev.Status = ElectricVehicleStatus.Pending;
                        _unitOfWork.ElectricVehicleRepository.Update(ev);
                    }
                }

                await _unitOfWork.BookingEVRepository.AddAsync(bookingEV, CancellationToken.None);

                await _unitOfWork.SaveAsync();
                await _logService.AddLogAsync(user,LogType.Create, "Booking", bookingEV.CreatedBy, ct);


                await _eContractService.CreateBookingEContractAsync(user, bookingEV.Id, ct);

                var bookingWithDetails = await _unitOfWork.BookingEVRepository.GetBookingWithIdAsync(bookingEV.Id);

                var getBookingEV = _mapper.Map<GetBookingEVDTO>(bookingWithDetails);

                var versionId = createBookingEVDTO.BookingDetails.First().VersionId;
                var colorId = createBookingEVDTO.BookingDetails.First().ColorId;
                var quantity = await _unitOfWork.ElectricVehicleRepository.GetAvailableQuantityByVersionColorAsync(versionId, colorId);
                await UpdateQuantityRealTime(versionId, colorId, quantity);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Booking created successfully",
                    StatusCode = 200,
                    Result = getBookingEV
                };

            }
            catch (Exception ex)
            {
                return new ResponseDTO()
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500,
                };
            }

        }

        public async Task<ResponseDTO> GetAllBookingEVsAsync(ClaimsPrincipal user, int pageNumber, int pageSize, BookingStatus? bookingStatus, CancellationToken ct)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userId == null)
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = false,
                        Message = "User not found",
                        StatusCode = 404
                    };
                }

                var role = user.FindFirst(ClaimTypes.Role)?.Value;

                var bookingEVs = new List<BookingEV>();
                Func<IQueryable<BookingEV>, IQueryable<BookingEV>> includes = q =>
                    q.Include(b => b.BookingEVDetails)
                        .ThenInclude(d => d.Color)
                    .Include(b => b.BookingEVDetails)
                        .ThenInclude(d => d.Version)
                    .Include(b => b.EContract)
                        .ThenInclude(ec => ec.Owner)
                    .AsNoTracking();

                (IReadOnlyList<BookingEV> items, int total) result;
                Expression<Func<BookingEV, bool>>? filter;
                if (role == StaticUserRole.Admin || role == StaticUserRole.EVMStaff)
                {
                    filter = null;
                    if (bookingStatus is not null)
                    {
                        filter = b => b.Status == bookingStatus;
                    }
                    result = await _unitOfWork.BookingEVRepository.GetPagedAsync(
                            filter: filter,
                            includes: includes,
                            orderBy: dm => dm.BookingDate,
                            ascending: false,
                            pageNumber: pageNumber,
                            pageSize: pageSize,
                            ct: ct);
                }
                else
                {
                    var dealer = await _unitOfWork.DealerRepository.GetManagerByUserIdAsync(userId, ct);

                    if (dealer is null)
                    {
                        dealer = await _unitOfWork.DealerRepository.GetDealerByUserIdAsync(userId, ct);
                        if (dealer is null)
                        {
                            return new ResponseDTO()
                            {
                                IsSuccess = false,
                                Message = "Dealer not found",
                                StatusCode = 404
                            };
                        }
                    }

                    filter = b => b.DealerId == dealer.Id;
                    if (bookingStatus is not null)
                    {
                        var dealerId = dealer.Id;
                        filter = b => b.DealerId == dealerId && b.Status == bookingStatus;
                    }

                    result = await _unitOfWork.BookingEVRepository.GetPagedAsync(
                            filter: filter,
                            includes: includes,
                            orderBy: dm => dm.BookingDate,
                            ascending: false,
                            pageNumber: pageNumber,
                            pageSize: pageSize,
                            ct: ct);
                }

                var getBookingEVs = _mapper.Map<List<GetBookingEVDTO>>(result.items);
                foreach (var booking in getBookingEVs)
                {
                    if (booking.EContract != null)
                    {
                        booking.EContract.CreatedName = (await _unitOfWork.UserManagerRepository.GetByIdAsync(booking.EContract.CreatedBy))?.FullName;
                    }
                }
                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Get all bookings successfully",
                    StatusCode = 200,
                    Result = new
                    {
                        data = getBookingEVs,
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
                return new ResponseDTO()
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500,
                };
            }
        }

        public async Task<ResponseDTO> GetBookingEVByIdAsync(Guid bookingId)
        {
            try
            {
                var bookingEV = await _unitOfWork.BookingEVRepository.GetBookingWithIdAsync(bookingId);
                if (bookingEV is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Booking not found",
                        StatusCode = 404
                    };
                }

                var getBookingEV = _mapper.Map<GetBookingEVDTO>(bookingEV);

                if (getBookingEV.EContract is not null)
                {
                    getBookingEV.EContract.CreatedName = (await _unitOfWork.UserManagerRepository.GetByIdAsync(getBookingEV.EContract.CreatedBy))?.FullName;
                }

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Booking retrieved successfully",
                    StatusCode = 200,
                    Result = getBookingEV
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO()
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500,
                };
            }
        }

        public async Task<ResponseDTO> GetVehicleByBookingIdAsync(Guid bookingId)
        {
            try
            {
                var vehicles = await _unitOfWork.BookingEVRepository.GetVehiclesByBookingIdAsync(bookingId);

                if (!vehicles.Any())
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "No vehicles found",
                        StatusCode = 404
                    };

                var result = _mapper.Map<List<GetVehicleByBookingDTO>>(vehicles);


                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Get vehicle from booking successfully",
                    StatusCode = 200,
                    Result = result
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
        public async Task<ResponseDTO> UpdateBookingStatusAsync(ClaimsPrincipal user, Guid bookingId, BookingStatus newStatus)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User not found.",
                        StatusCode = 404
                    };
                }

                var role = user.FindFirst(ClaimTypes.Role)?.Value;
                if (role == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User role not found.",
                        StatusCode = 403
                    };
                }

                var bookingEV = await _unitOfWork.BookingEVRepository.GetBookingWithIdAsync(bookingId);
                if (bookingEV == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Booking not found.",
                        StatusCode = 404
                    };
                }

                if (bookingEV.Status == BookingStatus.Cancelled ||
                    bookingEV.Status == BookingStatus.Rejected)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Cannot update status of a cancelled,rejected booking.",
                        StatusCode = 400
                    };
                }

                if (newStatus == BookingStatus.Pending && bookingEV.Status != BookingStatus.WaitingDealerSign)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Can only change to pending from waititingDealerSign",
                        StatusCode = 400
                    };
                }

                if (newStatus == BookingStatus.Approved || newStatus == BookingStatus.Rejected)
                {
                    if (role != StaticUserRole.Admin && role != StaticUserRole.EVMStaff)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Only Admin or EVM Staff can approve or reject a booking.",
                            StatusCode = 403
                        };
                    }
                }

                if (newStatus == BookingStatus.Cancelled)
                {
                    if (role != StaticUserRole.DealerManager)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Only Dealer Manager can cancel a booking.",
                            StatusCode = 403
                        };
                    }

                    if (bookingEV.Status != BookingStatus.WaitingDealerSign && bookingEV.Status != BookingStatus.Pending)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Can only cancel a pending or waitting booking.",
                            StatusCode = 400
                        };
                    }

                    foreach (var dt in bookingEV.BookingEVDetails)
                    {
                        var vehicles = await _unitOfWork.ElectricVehicleRepository
                            .GetPendingVehicleByModelVersionColorAsync(dt.Version.ModelId, dt.VersionId, dt.ColorId);

                        foreach (var ev in vehicles.Take(dt.Quantity))
                        {
                            ev.Status = ElectricVehicleStatus.Available;
                            _unitOfWork.ElectricVehicleRepository.Update(ev);
                        }
                    }

                    bookingEV.Status = BookingStatus.Cancelled;

                }

                if (newStatus == BookingStatus.Approved)
                {

                    foreach (var dt in bookingEV.BookingEVDetails)
                    {
                        var pendingVehicles = await _unitOfWork.ElectricVehicleRepository
                            .GetPendingVehicleByModelVersionColorAsync(dt.Version.ModelId, dt.VersionId, dt.ColorId);

                        if (pendingVehicles == null || !pendingVehicles.Any())
                        {
                            return new ResponseDTO
                            {
                                IsSuccess = false,
                                Message = "No available vehicles.",
                                StatusCode = 400
                            };
                        }

                        if (pendingVehicles.Count() < dt.Quantity)
                        {
                            return new ResponseDTO
                            {
                                IsSuccess = false,
                                Message = "Not enough vehicles available.",
                                StatusCode = 400
                            };
                        }

                        var selectedVehicles = pendingVehicles
                            .OrderBy(ev => ev.ImportDate)
                            .Take(dt.Quantity)
                            .ToList();

                        foreach (var ev in selectedVehicles)
                        {
                            ev.Status = ElectricVehicleStatus.Booked;
                            _unitOfWork.ElectricVehicleRepository.Update(ev);
                        }
                    }
                }

                if (newStatus == BookingStatus.Rejected)
                {
                    foreach (var dt in bookingEV.BookingEVDetails)
                    {
                        var pendingVehicles = await _unitOfWork.ElectricVehicleRepository
                            .GetPendingVehicleByModelVersionColorAsync(dt.Version.ModelId, dt.VersionId, dt.ColorId);

                        if (pendingVehicles == null || !pendingVehicles.Any())
                        {
                            return new ResponseDTO
                            {
                                IsSuccess = false,
                                Message = "No vehicles in pending status.",
                                StatusCode = 404
                            };
                        }

                        var selectedVehicles = pendingVehicles
                            .OrderBy(ev => ev.ImportDate)
                            .Take(dt.Quantity)
                            .ToList();

                        foreach (var ev in selectedVehicles)
                        {
                            ev.Status = ElectricVehicleStatus.Available;
                            _unitOfWork.ElectricVehicleRepository.Update(ev);
                        }
                    }
                }

                if (newStatus == BookingStatus.SignedByAdmin)
                {
                    if (role != StaticUserRole.Admin)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Only Admin can sign the booking.",
                            StatusCode = 403
                        };
                    }

                    if (bookingEV.Status != BookingStatus.Approved)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Booking must be approved by EVM Staff before Admin can sign.",
                            StatusCode = 400
                        };
                    }

                    await CreateVehicleDeliveryAsync(bookingEV);
                    await _logService.AddLogAsync(user, LogType.Create, "VehicleDelivery", bookingEV.Note, CancellationToken.None);
                }

                if (bookingEV.Status == BookingStatus.WaitingDealerSign && newStatus == BookingStatus.Pending)
                {
                    foreach (var dt in bookingEV.BookingEVDetails)
                    {
                        var availableVehicles = await _unitOfWork.ElectricVehicleRepository
                            .GetAvailableVehicleByModelVersionColorAsync(dt.Version.ModelId, dt.VersionId, dt.ColorId);

                        if (availableVehicles.Count() < dt.Quantity)
                            return new ResponseDTO
                            {
                                IsSuccess = false,
                                Message = "Not enough vehicles for booking.",
                                StatusCode = 400,
                            };

                        foreach (var ev in availableVehicles.Take(dt.Quantity))
                        {
                            ev.Status = ElectricVehicleStatus.Pending;
                            _unitOfWork.ElectricVehicleRepository.Update(ev);
                        }
                    }
                }


                bookingEV.Status = newStatus;
                _unitOfWork.BookingEVRepository.Update(bookingEV);
                await _unitOfWork.SaveAsync();
                await _logService.AddLogAsync(user, LogType.Update, "Booking", bookingEV.Note, CancellationToken.None);

                string message = newStatus switch
                {
                    BookingStatus.Approved => "Booking approved successfully.",
                    BookingStatus.Rejected => "Booking rejected successfully.",
                    BookingStatus.Cancelled => "Booking cancelled successfully.",
                    BookingStatus.Completed => "Booking completed successfully.",
                    _ => "Booking status updated successfully."
                };

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = message,
                    StatusCode = 200
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

        private async Task<ResponseDTO> CreateVehicleDeliveryAsync(BookingEV bookingEV)
        {
            var vehicleDelivery = new VehicleDelivery
            {
                BookingEVId = bookingEV.Id,
                Description = "Preparing vehicles to delivery",
                CreatedDate = DateTime.UtcNow,
                Status = DeliveryStatus.Preparing,
                UpdateAt = DateTime.UtcNow,
            };

            await _unitOfWork.VehicleDeliveryRepository.AddAsync(vehicleDelivery, CancellationToken.None);
            await _unitOfWork.SaveAsync();
            foreach (var dt in bookingEV.BookingEVDetails)
            {
                var bookedVehicles = await _unitOfWork.ElectricVehicleRepository
                    .GetBookedVehicleByModelVersionColorAsync(dt.Version.ModelId, dt.VersionId, dt.ColorId);

                if (bookedVehicles.Count() < dt.Quantity)
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = $"Not enough booked vehicles ",
                        StatusCode = 400
                    };

                var selectedVehicles = bookedVehicles
                    .OrderBy(ev => ev.ImportDate)
                    .Take(dt.Quantity)
                    .ToList();

                foreach (var ev in selectedVehicles)
                {
                    ev.Status = ElectricVehicleStatus.InTransit;
                    _unitOfWork.ElectricVehicleRepository.Update(ev);

                    var deliveryDetail = new VehicleDeliveryDetail
                    {
                        VehicleDeliveryId = vehicleDelivery.Id,
                        ElectricVehicleId = ev.Id,
                        Status = DeliveryVehicleStatus.Preparing,
                        Note = "Vehicle is being prepared for shipment"
                    };
                    await _unitOfWork.VehicleDeliveryDetailRepository.AddAsync(deliveryDetail, CancellationToken.None);
                }
            }

            await _unitOfWork.SaveAsync();

            var delivery = await _unitOfWork.VehicleDeliveryRepository.GetVehicleDeliveryById(vehicleDelivery.Id, CancellationToken.None);

            var getDelivery = _mapper.Map<GetVehicleDeliveryDTO>(delivery);
            return new ResponseDTO
            {
                IsSuccess = true,
                Message = "Create Vehicle Delivery successfully",
                StatusCode = 200
            };
        }


        private async Task<ResponseDTO> UpdateQuantityRealTime(Guid versionId, Guid colorId, int quantity)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveElectricVehicleQuantityUpdate", versionId, colorId, quantity);


            return new ResponseDTO
            {
                IsSuccess = true,
                Message = "Real-time quantity update sent successfully",
                StatusCode = 200
            };
        }
    }
}
