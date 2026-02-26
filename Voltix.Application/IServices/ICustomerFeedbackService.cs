using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.CustomerFeedback;
using Voltix.Application.DTO.DealerFeedBackDTO;
using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.IServices
{
    public interface ICustomerFeedbackService 
    {
        Task<ResponseDTO> CreateCustomerFeedbackAsync(ClaimsPrincipal user, CreateCustomerFeedbackDTO createCustomerFeedbackDTO);
        Task<ResponseDTO> GetAllCustomerFeedbacksAsync(ClaimsPrincipal user, CancellationToken ct);
        Task<ResponseDTO> GetCustomerFeedbackByIdAsync(ClaimsPrincipal user, Guid feedbackId);
        Task<ResponseDTO> UpdateCustomerFeedbackStatusAsync(ClaimsPrincipal user, Guid feedbackId, FeedbackStatus newStatus);
    }
}
