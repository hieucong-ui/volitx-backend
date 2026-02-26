using AutoMapper;
using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.Notification;
using Voltix.Application.IServices;
using Voltix.Domain.Constants;
using Voltix.Domain.Entities;
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
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public NotificationService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<ResponseDTO> GetAllNotification(ClaimsPrincipal userClaim, int pageNumber, int pageSize, CancellationToken ct)
        {
            try
            {
                var userId = userClaim.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var role = userClaim.FindFirst(ClaimTypes.Role)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User not found",
                        StatusCode = 404
                    };
                }
                Expression<Func<Notification, bool>>? filter = null;
                var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerIdAsync(userId, ct);
                if (dealer is null)
                {
                    dealer = await _unitOfWork.DealerRepository.GetDealerByUserIdAsync(userId, ct);
                    if (dealer is null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Dealer not found",
                            StatusCode = 404
                        };
                    }

                    filter = n => n.DealerId == dealer.Id && n.TargetRole == StaticUserRole.DealerStaff;
                }
                else
                {
                    filter = n => n.DealerId == dealer.Id && n.TargetRole == StaticUserRole.DealerManager;
                }

                (IReadOnlyList<Notification> items, int total) result;
                result = await _unitOfWork.NotificationRepository.GetPagedAsync(
                            filter: filter,
                            includes: null,
                            orderBy: dm => dm.CreatedAt,
                            ascending: false,
                            pageNumber: pageNumber,
                            pageSize: pageSize,
                            ct: ct);

                var getNotication = _mapper.Map<List<GetNotificationDTO>>(result.items);
                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Get notifications successfully",
                    Result = new
                    {
                        data = getNotication,
                        Pagination = new
                        {
                            PageNumber = pageNumber,
                            PageSize = pageSize,
                            TotalItems = result.total,
                            TotalPages = (int)Math.Ceiling((double)result.total / pageSize)
                        }
                    },
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Error to get all notification: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> ReadNotification(Guid notificationId, CancellationToken ct)
        {
            try
            {
                var notification = await _unitOfWork.NotificationRepository.GetByIdAsync(notificationId, ct);
                if (notification is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Notification not found",
                        StatusCode = 404
                    };
                }
                notification.IsRead = true;
                _unitOfWork.NotificationRepository.Update(notification);
                await _unitOfWork.SaveAsync();
                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Notification marked as read successfully",
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Error to mark notification as read: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> RealAll(ClaimsPrincipal userClaim, CancellationToken ct)
        {
            try
            {
                var userId = userClaim.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User not found",
                        StatusCode = 404
                    };
                }

                var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerIdAsync(userId, ct);
                if (dealer is null)
                {
                    dealer = await _unitOfWork.DealerRepository.GetDealerByUserIdAsync(userId, ct);
                    if (dealer is null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Dealer not found",
                            StatusCode = 404
                        };
                    }
                    await _unitOfWork.NotificationRepository.MarkAllAsReadAsync(dealer.Id, StaticUserRole.DealerStaff, ct);
                }
                else
                {
                    await _unitOfWork.NotificationRepository.MarkAllAsReadAsync(dealer.Id, StaticUserRole.DealerManager, ct);
                }

                await _unitOfWork.SaveAsync();
                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "All notifications marked as read successfully",
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Error to mark all notifications as read: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> UnReadNotification(Guid notificationId, CancellationToken ct)
        {
            try
            {
                var notification = await _unitOfWork.NotificationRepository.GetByIdAsync(notificationId, ct);
                if (notification is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Notification not found",
                        StatusCode = 404
                    };
                }

                notification.IsRead = false;
                _unitOfWork.NotificationRepository.Update(notification);
                await _unitOfWork.SaveAsync();
                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Notification marked as unread successfully",
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Error to mark notification as unread: {ex.Message}",
                    StatusCode = 500
                };
            }
        }
    }
}
