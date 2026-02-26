using Voltix.Application.DTO.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.IServices
{
    public interface IDashBoardService
    {
        Task<ResponseDTO> GetTotalCustomerAsync();
        Task<ResponseDTO> GetDealerDashboardAsync(ClaimsPrincipal user, CancellationToken ct);
        Task<ResponseDTO> GetDealerStaffDashboardAsync(ClaimsPrincipal user, CancellationToken ct);
        Task<ResponseDTO> GetDealerRevenueByQuarterAsync(ClaimsPrincipal user, int year, CancellationToken ct);
        Task<ResponseDTO> GetAdminDashboardAsync(CancellationToken ct);
    }
}
