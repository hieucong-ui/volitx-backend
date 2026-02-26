using AutoMapper;
using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.DealerFeedBackDTO;
using Voltix.Application.IServices;
using Voltix.Domain.Constants;
using Voltix.Domain.Entities;
using Voltix.Domain.Enums;
using Voltix.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.Services
{
    public class DealerFeedbackService : IDealerFeedbackService
    {
        public readonly IUnitOfWork _unitOfWork;
        public readonly IMapper _mapper;
        public readonly IS3Service _s3Service;

        public DealerFeedbackService(IUnitOfWork unitOfWork, IMapper mapper, IS3Service s3Service)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _s3Service = s3Service;
        }
        public async Task<ResponseDTO> CreateDealerFeedbackAsync(ClaimsPrincipal user, CreateDealerFeedBackDTO createDealerFeedBackDTO)
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

                var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerIdAsync(userId, CancellationToken.None);
                if (dealer == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Dealer not found ",
                        StatusCode = 404
                    };
                }

                DealerFeedback dealerFeedback = new DealerFeedback
                {
                    DealerId = dealer.Id,
                    FeedbackContent = createDealerFeedBackDTO.FeedbackContent,
                    Status = FeedbackStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };
                if (dealerFeedback == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Dealer feedback is null",
                        StatusCode = 404
                    };
                }

                if (createDealerFeedBackDTO.AttachmentKeys != null && createDealerFeedBackDTO.AttachmentKeys.Any())
                {
                    foreach (var key in createDealerFeedBackDTO.AttachmentKeys)
                    {
                        var fileName = Path.GetFileName(key);
                        dealerFeedback.DealerFBAttachments.Add(new DealerFBAttachment
                        {
                            FileName = fileName,
                            Key = key
                        });
                    }
                }

                await _unitOfWork.DealerFeedbackRepository.AddAsync(dealerFeedback, CancellationToken.None);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Create dealer feedback successfully",
                    StatusCode = 201
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

        public async Task<ResponseDTO> GetAllDealerFeedbacksAsync(ClaimsPrincipal user, CancellationToken ct)
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
                var dealerFeedbacks = new List<DealerFeedback>();

                Dealer? dealer = null;
                if (role != StaticUserRole.Admin && role != StaticUserRole.EVMStaff)
                {
                    dealer = await _unitOfWork.DealerRepository.GetDealerByManagerIdAsync(userId, ct);
                    if (dealer == null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Dealer not found",
                            StatusCode = 404
                        };
                    }
                }

                if (role == StaticUserRole.Admin || role == StaticUserRole.EVMStaff)
                {
                    dealerFeedbacks = (await _unitOfWork.DealerFeedbackRepository.GetAllDealerFeedbacksWithDetailAsync(ct)).ToList();
                }
                else
                {
                    dealerFeedbacks = (await _unitOfWork.DealerFeedbackRepository.GetAllDealerFeedbacksWithDetailAsync(ct))
                                        .Where(df => df.DealerId == dealer.Id)
                                        .ToList();
                }
                var getDealerFeedbacks = _mapper.Map<List<GetDealerFeedBackDTO>>(dealerFeedbacks);

                foreach (var fb in getDealerFeedbacks)
                {
                    var attachments = _unitOfWork.DealerFBAttachmentRepository
                        .GetAttachmentsByDealerFbId(fb.Id);

                    var urlLists = new List<string>();
                    foreach (var att in attachments)
                    {
                        var url = _s3Service.GenerateDownloadUrl(att.Key);
                        urlLists.Add(url);
                    }

                    fb.ImgUrls = urlLists;
                }

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Get all dealer feedbacks successfully",
                    StatusCode = 200,
                    Result = getDealerFeedbacks
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
        public async Task<ResponseDTO> GetDealerFeedbackByIdAsync(Guid feedbackId)
        {
            try
            {
                var dealerFeedback = await _unitOfWork.DealerFeedbackRepository.GetFeedbackByIdAsync(feedbackId);
                if (dealerFeedback == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Dealer feedback not found",
                        StatusCode = 404
                    };
                }


                var getDealerFeedback = _mapper.Map<GetDealerFeedBackDTO>(dealerFeedback);

                var attachments = _unitOfWork.DealerFBAttachmentRepository
                    .GetAttachmentsByDealerFbId(getDealerFeedback.Id);

                var urlLists = new List<string>();
                foreach (var att in attachments)
                {
                    var url = _s3Service.GenerateDownloadUrl(att.Key);
                    urlLists.Add(url);
                }
                getDealerFeedback.ImgUrls = urlLists;

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Get dealer feedback by id successfully",
                    StatusCode = 200,
                    Result = getDealerFeedback
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

        public async Task<ResponseDTO> UpdateDealerFeedbackStatusAsync(ClaimsPrincipal user, Guid feedbackId, FeedbackStatus newStatus)
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

                Dealer? dealer = null;
                if (role == StaticUserRole.DealerManager)
                {
                    dealer = await _unitOfWork.DealerRepository
                        .GetDealerByManagerOrStaffAsync(userId, CancellationToken.None);

                    if (dealer == null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Dealer not found.",
                            StatusCode = 404
                        };
                    }
                }

                var dealerFeedback = await _unitOfWork.DealerFeedbackRepository.GetFeedbackByIdAsync(feedbackId);
                if (dealerFeedback == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Dealer feedback not found.",
                        StatusCode = 404
                    };
                }

                if (role == StaticUserRole.DealerManager)
                {
                    if (dealerFeedback.DealerId != dealer!.Id)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "You cannot modify feedback from another dealer.",
                            StatusCode = 403
                        };
                    }

                    if (dealerFeedback.Status != FeedbackStatus.Pending)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "You can only cancel feedbacks that are still pending.",
                            StatusCode = 400
                        };
                    }

                    if (newStatus != FeedbackStatus.Cancelled)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Dealer Manager can only cancel feedbacks.",
                            StatusCode = 403
                        };
                    }
                }
                else if (role == StaticUserRole.Admin || role == StaticUserRole.EVMStaff)
                {
                    bool isInvalidTransition = dealerFeedback.Status switch
                    {
                        FeedbackStatus.Pending => !(newStatus is FeedbackStatus.Accepted or FeedbackStatus.Rejected),
                        FeedbackStatus.Accepted => newStatus != FeedbackStatus.Replied,
                        FeedbackStatus.Rejected => newStatus != FeedbackStatus.Replied,
                        _ => true
                    };

                    if (isInvalidTransition)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = $"Invalid transition from {dealerFeedback.Status} to {newStatus}.",
                            StatusCode = 400
                        };
                    }
                }
                else
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "You do not have permission to update dealer feedback.",
                        StatusCode = 403
                    };
                }

                dealerFeedback.Status = newStatus;
                _unitOfWork.DealerFeedbackRepository.Update(dealerFeedback);
                await _unitOfWork.SaveAsync();

                string successMessage = newStatus switch
                {
                    FeedbackStatus.Accepted => "Dealer feedback accepted successfully.",
                    FeedbackStatus.Rejected => "Dealer feedback rejected successfully.",
                    FeedbackStatus.Replied => "Dealer feedback replied successfully.",
                    FeedbackStatus.Cancelled => "Dealer feedback cancelled successfully.",
                    _ => "Dealer feedback status updated successfully."
                };

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = successMessage,
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

    }
}