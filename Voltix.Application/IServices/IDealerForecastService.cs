using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.Dealer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.IServices
{
    public interface IDealerForecastService
    {
        Task<ResponseDTO> BuildDailySnapshotAsync(DateTime utcDate, CancellationToken ct);
        Task<ResponseDTO> GetDemandSeriesAsync(ClaimsPrincipal userClaim, Guid? dealerId, Guid evTemplateId, DateTime from, DateTime to, CancellationToken ct);
        Task<ResponseDTO> UpsertForecastBatchAsync(IEnumerable<UpsertDealerInventoryForecastDTO> forecasts, CancellationToken ct);
        Task<ResponseDTO> EvaluateInventoryRiskAsync(int horizonDays, CancellationToken ct);
        Task<ResponseDTO> GetForecastTargetsAsync(CancellationToken ct);
        Task<ResponseDTO> GetForecastSeriesAsync(ClaimsPrincipal userClaim, Guid? dealerId, Guid evTemplateId, DateTime from, DateTime to, CancellationToken ct);
    }
}
