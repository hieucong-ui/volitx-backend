using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.Dealer;
using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.IServices
{
    public interface IDealerService
    {
        Task<ResponseDTO> CreateDealerStaffAsync(ClaimsPrincipal user, CreateDealerStaffDTO createDealerStaffDTO, CancellationToken ct);
        Task<ResponseDTO> GetAllDealerStaffAsync(ClaimsPrincipal claimUser, string? filterOn, string? filterQuery, string? sortBy, bool? isAcsending, int pageNumber, int PageSize, CancellationToken ct);
        Task<ResponseDTO> GetAllDealerAsync(string? filterOn, string? filterQuery, string? sortBy, DealerStatus? status, bool? isAcsending, int pageNumber, int PageSize, CancellationToken ct);        Task<ResponseDTO> DealerInformationAsync(ClaimsPrincipal claimUser, CancellationToken ct);
        Task<ResponseDTO> UpdateStatusDealer(Guid DealerId, DealerStatus status, CancellationToken ct);
        Task<ResponseDTO> UpdateStatusDealerStaff(ClaimsPrincipal userClaim, bool isActive, string applicationUserId, CancellationToken ct);
    }

}
