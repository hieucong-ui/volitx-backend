using AutoMapper;
using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.CustomerFeedback;
using Voltix.Application.IServices;
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
    public class CustomerFeedbackService : ICustomerFeedbackService
    {
        public readonly IUnitOfWork _unitOfWork;
        public readonly IMapper _mapper;
        public readonly IS3Service _s3Service;
        public CustomerFeedbackService(IUnitOfWork unitOfWork, IMapper mapper, IS3Service s3Service)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _s3Service = s3Service;
        }
        public async Task<ResponseDTO> CreateCustomerFeedbackAsync(ClaimsPrincipal user, CreateCustomerFeedbackDTO createCustomerFeedbackDTO)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 401,
                        Message = "User not login yet."
                    };
                }

                var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerOrStaffAsync(userId, CancellationToken.None);
                if (dealer is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 403,
                        Message = "Dealer not found ."
                    };
                }

                CustomerFeedback customerFeedback = new CustomerFeedback
                {
                    DealerId = dealer.Id,
                    CustomerId = createCustomerFeedbackDTO.CustomerId,
                    FeedbackContent = createCustomerFeedbackDTO.FeedbackContent,
                    Status = FeedbackStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                };

                if (createCustomerFeedbackDTO.AttachmentKeys != null && createCustomerFeedbackDTO.AttachmentKeys.Any())
                {
                    foreach (var key in createCustomerFeedbackDTO.AttachmentKeys)
                    {
                        var fileName = Path.GetFileName(key);
                        customerFeedback.CustomerFBAttachments.Add(new CustomerFBAttachment
                        {
                            FileName = fileName,
                            Key = key
                        });
                    }
                }

                await _unitOfWork.CustomerFeedbackRepository.AddAsync(customerFeedback, CancellationToken.None);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 201,
                    Message = "Customer feedback created successfully."
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResponseDTO> GetAllCustomerFeedbacksAsync(ClaimsPrincipal user, CancellationToken ct)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 401,
                        Message = "User not login yet."
                    };
                }

                var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerOrStaffAsync(userId, ct);
                if (dealer is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 403,
                        Message = "Dealer not found ."
                    };
                }

                var customerFeedbacks = await _unitOfWork.CustomerFeedbackRepository.GetFeedbacksByDealerIdAsync(dealer.Id);

                var getCustomerFeedbackDTOs = _mapper.Map<List<GetCustomerFeedbackDTO>>(customerFeedbacks);

                foreach (var fb in getCustomerFeedbackDTOs)
                {
                    var attachments = _unitOfWork.CustomerFBAttachRepository
                        .GetAttachmentsByCustomerFbId(fb.Id);

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
                    StatusCode = 200,
                    Message = "Customer feedbacks retrieved successfully.",
                    Result = getCustomerFeedbackDTOs
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResponseDTO> GetCustomerFeedbackByIdAsync(ClaimsPrincipal user, Guid feedbackId)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId is null)
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 401,
                        Message = "User not login yet."
                    };

                var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerOrStaffAsync(userId, CancellationToken.None);
                if (dealer is null)
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 403,
                        Message = "Dealer not found."
                    };

                var feedback = await _unitOfWork.CustomerFeedbackRepository.GetFeedbackByIdAsync(feedbackId);
                if (feedback is null)
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Customer feedback not found."
                    };

                // Only dealer who owns the feedback can access it
                if (feedback.DealerId != dealer.Id)
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 403,
                        Message = "You don't have permission to access this feedback."
                    };

                var getCustomerFeedback = _mapper.Map<GetCustomerFeedbackDTO>(feedback);
                var attachments = _unitOfWork.CustomerFBAttachRepository
                .GetAttachmentsByCustomerFbId(getCustomerFeedback.Id);

                var imgUrls = new List<string>();
                foreach (var att in attachments)
                {
                    var url = _s3Service.GenerateDownloadUrl(att.Key);
                    imgUrls.Add(url);
                }

                getCustomerFeedback.ImgUrls = imgUrls;

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Customer feedback retrieved successfully.",
                    Result = getCustomerFeedback
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = ex.Message
                };
            }
        }


        public async Task<ResponseDTO> UpdateCustomerFeedbackStatusAsync(ClaimsPrincipal user, Guid feedbackId, FeedbackStatus newStatus)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User not login yet.",
                        StatusCode = 401,
                    };
                }


                var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerOrStaffAsync(userId, CancellationToken.None);
                if (dealer is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Dealer not found.",
                        StatusCode = 403,
                    };
                }


                var customerFeedback = await _unitOfWork.CustomerFeedbackRepository.GetFeedbackByIdAsync(feedbackId);
                if (customerFeedback is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Feedback not found.",
                        StatusCode = 404,
                    };
                }


                // Access control: only the dealer who owns the feedback can update it
                if (customerFeedback.DealerId != dealer.Id)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "You don't have permission to update this feedback.",
                        StatusCode = 403,
                    };
                }

                // Check current status
                if (customerFeedback.Status != FeedbackStatus.Pending)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Only feedbacks with 'Pending' status can be updated.",
                        StatusCode = 400,
                    };
                }


                // Dealers can only update status to Replied or Cancelled
                if (newStatus != FeedbackStatus.Replied && newStatus != FeedbackStatus.Cancelled)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Invalid status update. Dealers can only set status to 'Replied' or 'Cancelled'.",
                        StatusCode = 400,
                    };
                }
                   

                customerFeedback.Status = newStatus;

                _unitOfWork.CustomerFeedbackRepository.Update(customerFeedback);
                await _unitOfWork.SaveAsync();

                string message = newStatus switch
                {
                    FeedbackStatus.Replied => "Feedback has been replied successfully.",
                    FeedbackStatus.Cancelled => "Feedback has been cancelled successfully.",
                    _ => "Feedback status updated successfully."
                };

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = message,
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
