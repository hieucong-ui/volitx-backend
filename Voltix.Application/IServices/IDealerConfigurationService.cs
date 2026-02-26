using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.DealerConfiguration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.IServices
{
    public interface IDealerConfigurationService
    {
        Task<ResponseDTO<GetDealerConfigurationDTO>> GetCurrentConfigurationAsync(ClaimsPrincipal userClaim, CancellationToken ct);
        Task<ResponseDTO> UpsertConfigurationAsync(ClaimsPrincipal userClaim, UpsertDealerConfigurationDTO dto, CancellationToken ct);
        Task<ResponseDTO> UpdateAllDepositSettingsAsync(ClaimsPrincipal userClaim, UpdateAllDepositSettingsDTO dto, CancellationToken ct);
        Task<ResponseDTO> GenerateTimeSlotAsync(ClaimsPrincipal userClaim, DateTime? targetDate = null, CancellationToken ct = default);
        Task<ResponseDTO> GetDefaultConfigurationAsync(CancellationToken ct);
    }
}
