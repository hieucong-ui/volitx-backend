using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.VehicleDelivery;
using Voltix.Application.IServices;
using Voltix.Domain.Constants;
using Voltix.Domain.Entities;
using Voltix.Domain.Enums;
using Voltix.Infrastructure.IRepository;
using Voltix.Infrastructure.SignlR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.Services
{
    public class VehicleDeliveryService : IVehicleDeliveryService
    {
        public readonly IUnitOfWork _unitOfWork;
        public readonly IMapper _mapper;
        public readonly IBookingEVService _bookingEVService;
        public readonly ILogService _logService;
        private readonly IHubContext<NotificationHub> _hubContext;

        public VehicleDeliveryService(IUnitOfWork unitOfWork, IMapper mapper, IBookingEVService bookingEVService, ILogService logService, IHubContext<NotificationHub> hubContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _bookingEVService = bookingEVService;
            _logService = logService;
            _hubContext = hubContext;
        }
        public async Task<ResponseDTO> GetAllVehicleDelivery(ClaimsPrincipal user, int pageNumber, int pageSize, DeliveryStatus? status, Guid? templateId,bool isShow, CancellationToken ct)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var role = user.FindFirst(ClaimTypes.Role)?.Value;

                if (userId == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User not found.",
                        StatusCode = 401
                    };
                }

                Guid? dealerId = null;
                if (role == StaticUserRole.DealerManager)
                {
                    var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerIdAsync(userId, ct);
                    if (dealer == null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Dealer not found for current user.",
                            StatusCode = 404
                        };
                    }
                    dealerId = dealer.Id;
                }

                Func<IQueryable<VehicleDelivery>, IQueryable<VehicleDelivery>> includes = q => q
                    .Include(vd => vd.BookingEV)
                        .ThenInclude(b => b.Dealer)
                    .Include(vd => vd.VehicleDeliveryDetails)
                        .ThenInclude(vdd => vdd.ElectricVehicle)
                            .ThenInclude(vdd => vdd.ElectricVehicleTemplate)
                                .ThenInclude(evt => evt.Version)
                    .Include(vd => vd.VehicleDeliveryDetails)
                        .ThenInclude(vdd => vdd.ElectricVehicle)
                            .ThenInclude(vdd => vdd.ElectricVehicleTemplate)
                                .ThenInclude(evt => evt.Color);



                Expression<Func<VehicleDelivery, bool>>? filter = null;
                if (status.HasValue || templateId.HasValue || dealerId.HasValue)
                {
                    filter = vd =>
                        (!status.HasValue || vd.Status == status.Value) &&
                        (!templateId.HasValue ||
                            vd.VehicleDeliveryDetails.Any(vdd =>
                                vdd.ElectricVehicle != null &&
                                vdd.ElectricVehicle.ElectricVehicleTemplateId == templateId.Value)) &&
                                (!dealerId.HasValue || vd.BookingEV.DealerId == dealerId.Value);
                }

                (IReadOnlyList<VehicleDelivery> items, int total) result;
                result = await _unitOfWork.VehicleDeliveryRepository.GetPagedAsync(
                            filter: filter,
                            includes: includes,
                            orderBy: dm => dm.CreatedDate,
                            ascending: false,
                            pageNumber: pageNumber,
                            pageSize: pageSize,
                            ct: ct);

                if (result.items == null || !result.items.Any())
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "No vehicle deliveries found for the specified criteria.",
                        StatusCode = 404
                    };
                }

                var deliveries = result.items.ToList();

                var templateSummary = deliveries
                    .SelectMany(d => d.VehicleDeliveryDetails)
                    .Where(vdd => vdd.ElectricVehicle != null && vdd.ElectricVehicle.ElectricVehicleTemplateId != null)
                    .GroupBy(vdd => vdd.ElectricVehicle.ElectricVehicleTemplateId)
                    .Select(g => new
                    {
                        VersionName = g.First().ElectricVehicle.ElectricVehicleTemplate.Version.VersionName,
                        ColorName = g.First().ElectricVehicle.ElectricVehicleTemplate.Color.ColorName,
                        TemplateId = g.Key,
                        VehicleCount = g.Count(),
                        VinList = g.Select(vdd => vdd.ElectricVehicle.VIN).ToList()
                    })
                    .ToList();

                var getDeliveries = _mapper.Map<List<GetVehicleDeliveryDTO>>(result.items);
                if (isShow)
                {
                    foreach(var d in getDeliveries)
                    {
                        d.VehicleDeliveryDetails = d.VehicleDeliveryDetails
                            .Where(vdd => vdd.Status != DeliveryVehicleStatus.Damaged)
                            .ToList();
                    }

                    getDeliveries = getDeliveries
                        .Where(d => d.VehicleDeliveryDetails != null
                        && d.VehicleDeliveryDetails.Any())
                        .ToList();
                }

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Get all deliveries successfully",
                    StatusCode = 200,
                    Result = new
                    {
                        data = getDeliveries,
                        templateSummary,
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
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500,
                };
            }
        }

        public async Task<ResponseDTO> GetVehicleDeliveryById(Guid deliveryId , CancellationToken ct)
        {
            try
            {
                var delivery = await _unitOfWork.VehicleDeliveryRepository.GetVehicleDeliveryById(deliveryId,ct);
                if (delivery == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Delivery not found",
                        StatusCode = 400,
                    };
                }

                var getDelivery = _mapper.Map<GetVehicleDeliveryDTO>(delivery);
                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Get Delivery successfully",
                    StatusCode = 200,
                    Result = getDelivery
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

        public async Task<ResponseDTO> UpdateVehicleDeliveryStatus(ClaimsPrincipal user, Guid deliveryId, DeliveryStatus newStatus, CancellationToken ct, string? reason = null)
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

                var role = user.FindFirst(ClaimTypes.Role)?.Value;
                if (role != StaticUserRole.Admin && role != StaticUserRole.EVMStaff && role != StaticUserRole.DealerManager)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Only Admin , Dealer Manager or EVM Staff can update delivery status",
                        StatusCode = 403
                    };
                }

                var delivery = await _unitOfWork.VehicleDeliveryRepository.GetVehicleDeliveryById(deliveryId, ct);
                if (delivery == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Delivery not found",
                        StatusCode = 404
                    };
                }

                if (delivery.Status == DeliveryStatus.Preparing && newStatus != DeliveryStatus.Packing)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Delivery can only move from Preparing to Packing",
                        StatusCode = 400
                    };
                }

                if (delivery.Status == DeliveryStatus.Packing && newStatus != DeliveryStatus.InTransit)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Delivery can only move from Packing to InTransit",
                        StatusCode = 400
                    };
                }

                if (delivery.Status == DeliveryStatus.InTransit &&
                    !(newStatus == DeliveryStatus.InTransit 
                    || newStatus == DeliveryStatus.Accident 
                    || newStatus == DeliveryStatus.Delayed
                    || newStatus == DeliveryStatus.Arrived))
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "From InTransit, status can only change to InTransit, Arrived, Accident or Delayed",
                        StatusCode = 400
                    };
                }

                if (delivery.Status == DeliveryStatus.Arrived &&
                    newStatus != DeliveryStatus.Confirmed)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "After Arrived, delivery can only move to Confirmed",
                        StatusCode = 400
                    };
                }

                switch (newStatus)
                {
                    case DeliveryStatus.Preparing:
                        delivery.Description = "Chuẩn bị xe để vận chuyển";
                        break;
                    case DeliveryStatus.Packing:
                        delivery.Description = "Xe đang được đóng gói";
                        break;
                    case DeliveryStatus.InTransit:
                        delivery.Description = "Xe đang trên đường vận chuyển";
                        break;
                    case DeliveryStatus.Arrived:
                        delivery.Description = "Xe đã đến đại lý";
                        break;
                    case DeliveryStatus.Confirmed:
                        delivery.Description = "Giao nhận hoàn tất";
                        break;
                    case DeliveryStatus.Delayed:
                    case DeliveryStatus.Accident:
                        if (string.IsNullOrWhiteSpace(reason))
                        {
                            return new ResponseDTO
                            {
                                IsSuccess = false,
                                Message = "Bạn phải nhập lý do khi có tai nạn hoặc delay",
                                StatusCode = 400
                            };
                        }
                        delivery.Description = reason;
                        break;
                }

                delivery.Status = newStatus;
                delivery.UpdateAt = DateTime.UtcNow;
                _unitOfWork.VehicleDeliveryRepository.Update(delivery);

                foreach (var dt in delivery.VehicleDeliveryDetails)
                {
                    switch (newStatus)
                    {
                        case DeliveryStatus.Packing:
                            dt.Status = DeliveryVehicleStatus.Preparing;
                            dt.Note = "Vehicle is being prepared for shipment";
                            break;

                        case DeliveryStatus.InTransit:
                            dt.Status = DeliveryVehicleStatus.InTransit;
                            dt.Note = "Vehicle is on the way to dealer";
                            break;

                        case DeliveryStatus.Arrived:
                            dt.Status = DeliveryVehicleStatus.Delivered;
                            dt.Note = "Vehicle has arrived at dealer";
                            break;
                    }

                    _unitOfWork.VehicleDeliveryDetailRepository.Update(dt);
                }

                if (newStatus == DeliveryStatus.Accident)
                {
                    await ReportAccidentAsync(delivery, reason!);
                }
                else if (newStatus == DeliveryStatus.Confirmed)
                {
                    await _bookingEVService.ConfirmBookingDeliveryAsync(user, delivery.BookingEV.Id, ct);
                }

                await _unitOfWork.SaveAsync();
                await _logService.AddLogAsync(user, LogType.Update, "VehicleDelivery", delivery.Description, CancellationToken.None);


                var getDelivery = _mapper.Map<GetVehicleDeliveryDTO>(delivery);

                await UpdateStatusRealTime(delivery.BookingEV.DealerId);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = $"Delivery status updated to {newStatus}",
                    StatusCode = 200,
                    Result = getDelivery
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

        private async Task<ResponseDTO> ReportAccidentAsync(VehicleDelivery delivery, string reason)
        {
            delivery.Status = DeliveryStatus.Accident;
            delivery.Description = reason;
            delivery.UpdateAt = DateTime.UtcNow;
            _unitOfWork.VehicleDeliveryRepository.Update(delivery);
            await _unitOfWork.SaveAsync();

            return new ResponseDTO
            {
                IsSuccess = true,
                Message = "Accident report successfully",
                StatusCode = 200
            };

        }

        public async Task<ResponseDTO> InspectAccidentVehicleAsync(ClaimsPrincipal user, Guid deliveryId, List<Guid> damagedVehicleIds,bool isShow ,CancellationToken ct)
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
                if (role != StaticUserRole.Admin && role != StaticUserRole.EVMStaff)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Only EVMStaff and Admin can inspect accident vehicles",
                        StatusCode = 403
                    };
                }

                var delivery = await _unitOfWork.VehicleDeliveryRepository.GetVehicleDeliveryById(deliveryId, ct);
                if (delivery == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Delivery not found",
                        StatusCode = 400
                    };
                }

                foreach (var dt in delivery.VehicleDeliveryDetails)
                {
                    if (damagedVehicleIds.Contains(dt.ElectricVehicleId))
                    {
                        dt.Status = DeliveryVehicleStatus.Damaged;
                        dt.ElectricVehicle.Status = ElectricVehicleStatus.Maintenance;
                        dt.Note = "Vehicle damaged in accident, needs replacement.";
                    }

                    _unitOfWork.VehicleDeliveryDetailRepository.Update(dt);
                    _unitOfWork.ElectricVehicleRepository.Update(dt.ElectricVehicle);
                }

                delivery.Status = damagedVehicleIds.Any() ? DeliveryStatus.Accident : DeliveryStatus.InTransit;
                delivery.UpdateAt = DateTime.UtcNow;
                _unitOfWork.VehicleDeliveryRepository.Update(delivery);

                await _unitOfWork.SaveAsync();
                var vehicleList = delivery.VehicleDeliveryDetails.AsEnumerable();

                //Only show non-damaged vehicles
                if (isShow)
                {
                    vehicleList = vehicleList
                        .Where(dt => dt.Status != DeliveryVehicleStatus.Damaged); 
                }

                var result = vehicleList
                    .Select(dt => new
                    {
                        dt.ElectricVehicleId,
                        dt.ElectricVehicle.VIN,
                        dt.Note
                    })
                    .ToList();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Inspection successfully",
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

        public async Task<ResponseDTO> ReplaceDamagedVehicleAsync(ClaimsPrincipal user, Guid deliveryId, CancellationToken ct)
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
                        StatusCode = 401
                    };
                }

                var role = user.FindFirst(ClaimTypes.Role)?.Value;
                if(role != StaticUserRole.Admin && role != StaticUserRole.EVMStaff)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Only EVMStaff or Admin can replace vehicle",
                        StatusCode = 404
                    };
                }

                var delivery = await _unitOfWork.VehicleDeliveryRepository.GetVehicleDeliveryById(deliveryId, ct);
                if(delivery == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Delivery not found",
                        StatusCode = 404
                    };
                }

                var damagedVehicle = delivery.VehicleDeliveryDetails
                    .Where(v => v.Status == DeliveryVehicleStatus.Damaged)
                    .ToList();
                if (!damagedVehicle.Any())
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "No damaged vehicle found",
                        StatusCode = 400
                    };
                }
                 
                var usedReplacementIds = new HashSet<Guid>();

                foreach (var dt in damagedVehicle)
                {
                    var template = dt.ElectricVehicle.ElectricVehicleTemplate;

                    var replacement = await _unitOfWork.ElectricVehicleRepository
                        .GetFirstAvailableVehicleAsync(template.VersionId, template.ColorId, usedReplacementIds, ct);
                    if(replacement == null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "No available replacement vehicle found for delivery",
                            StatusCode = 400
                        };
                    }

                    dt.ElectricVehicleId = replacement.Id;
                    dt.Status = DeliveryVehicleStatus.InTransit;
                    dt.Note = $"Replaced damaged vehicle with VIN {replacement.VIN}";
                    replacement.Status = ElectricVehicleStatus.InTransit;

                    _unitOfWork.ElectricVehicleRepository.Update(replacement);
                    _unitOfWork.VehicleDeliveryDetailRepository.Update(dt);

                    usedReplacementIds.Add(replacement.Id);
                }

                delivery.Status = DeliveryStatus.InTransit;
                delivery.Description = "Vehicle is on the way to dealer";
                delivery.UpdateAt = DateTime.UtcNow;
                _unitOfWork.VehicleDeliveryRepository.Update(delivery); 
                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Replace damaged vehicle successfully",
                    StatusCode = 200
                };

            }
            catch(Exception ex) 
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500
                };
            }
        }

        private async Task<ResponseDTO> UpdateStatusRealTime(Guid dealerId)
        {
            var groupName = $"dealer:{dealerId}:all";

            await _hubContext.Clients
                .Group(groupName)
                .SendAsync("ReceiveVehicleDeliveryStatusUpdate");

            return new ResponseDTO
            {
                IsSuccess = true,
                Message = "Real-time update delivery vehicle status sent successfully",
                StatusCode = 200
            };
        }
    }
}
