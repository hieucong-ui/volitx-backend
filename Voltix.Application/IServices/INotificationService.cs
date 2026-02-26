using Voltix.Application.DTO.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.IServices
{
    public interface INotificationService
    {
        Task<ResponseDTO> GetAllNotification(ClaimsPrincipal userClaim, int pageNumber, int pageSize, CancellationToken ct);
        Task<ResponseDTO> ReadNotification(Guid notificationId, CancellationToken ct);
        Task<ResponseDTO> RealAll(ClaimsPrincipal userClaim, CancellationToken ct);
    }
}
