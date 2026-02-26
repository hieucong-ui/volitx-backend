using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Voltix.Application.DTO.Dealer;
using Voltix.Application.IServices;
using Voltix.Domain.Constants;
using Voltix.Domain.Enums;

namespace Voltix.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DealerController : ControllerBase
    {
        private readonly IDealerService _dealerService;
        private readonly IDealerTierService _dealerTierService;
        private readonly IDealerForecastService _dealerForecastService;
        public DealerController(IDealerService dealerService, IDealerTierService dealerTierService, IDealerForecastService dealerDailyInventoryService)
        {
            _dealerService = dealerService;
            _dealerTierService = dealerTierService;
            _dealerForecastService = dealerDailyInventoryService;
        }

        [HttpPost]
        [Route("create-dealer-staff")]
        [Authorize(Roles = StaticUserRole.DealerManager)]
        public async Task<IActionResult> CreateDealerStaff([FromBody] CreateDealerStaffDTO createDealerStaffDTO, CancellationToken ct)
        {
            var response = await _dealerService.CreateDealerStaffAsync(User, createDealerStaffDTO, ct);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet]
        [Route("get-all-dealer-staff")]
        [Authorize(Roles = StaticUserRole.DealerManager)]
        public async Task<IActionResult> GetAllDealerStaff([FromQuery] string? filterOn, [FromQuery] string? filterQuery,
            [FromQuery] string? sortBy, [FromQuery] bool? isAcsending, [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10, CancellationToken ct = default)
        {
            var response = await _dealerService.GetAllDealerStaffAsync(User, filterOn, filterQuery, sortBy, isAcsending, pageNumber, pageSize, ct);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet]
        [Route("get-all-dealers")]
        //[Authorize(Roles = StaticUserRole.Admin)]
        public async Task<IActionResult> GetAllDealers([FromQuery] string? filterOn, [FromQuery] string? filterQuery,
            [FromQuery] string? sortBy, [FromQuery] DealerStatus? status, [FromQuery] bool? isAcsending, [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10, CancellationToken ct = default)
        {
            var response = await _dealerService.GetAllDealerAsync(filterOn, filterQuery, sortBy, status, isAcsending, pageNumber, pageSize, ct);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut]
        [Route("update-dealer-tier/{dealerTierId}")]
        //[Authorize(Roles = StaticUserRole.Admin)]
        public async Task<IActionResult> UpdateDealerTier([FromRoute] Guid dealerTierId, [FromBody] UpdateDealerTierDTO updateDealer, CancellationToken ct)
        {
            var response = await _dealerTierService.UpdateDealerTier(dealerTierId, updateDealer, ct);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet]
        [Route("get-all-dealer-tiers")]
        //[Authorize(Roles = StaticUserRole.Admin)]
        public async Task<IActionResult> GetAllDealerTiers(CancellationToken ct)
        {
            var response = await _dealerTierService.GetAllDealerTiers(ct);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost]
        [Route("create-dealer-policy-override/{dealerId}")]
        //[Authorize(Roles = StaticUserRole.Admin)]
        public async Task<IActionResult> CreateDealerPolicyOverride([FromRoute] Guid dealerId, [FromBody] CreateDealerPolicyOverrideDTO createDealerPolicy, CancellationToken ct)
        {
            var response = await _dealerTierService.CreateDealerPolicyOverrideAsync(dealerId, createDealerPolicy, ct);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet]
        [Route("get-effective-policy")]
        //[Authorize(Roles = StaticUserRole.Admin)]
        public async Task<IActionResult> GetEffectivePolicy([FromQuery] Guid dealerId, CancellationToken ct)
        {
            var response = await _dealerTierService.GetEffectivePolicyAsync(dealerId, ct);
            return StatusCode(200, response);
        }

        [HttpGet]
        [Route("dealer-information")]
        [Authorize(Roles = StaticUserRole.DealerManager)]
        public async Task<IActionResult> DealerInformation(CancellationToken ct)
        {
            var response = await _dealerService.DealerInformationAsync(User, ct);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut]
        [Route("update-dealer-status/{dealerId}")]
        //[Authorize(Roles = StaticUserRole.Admin)]
        public async Task<IActionResult> UpdateStatusDealer([FromRoute] Guid dealerId, [FromQuery] DealerStatus status, CancellationToken ct)
        {
            var response = await _dealerService.UpdateStatusDealer(dealerId, status, ct);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut]
        [Route("update-dealer-staff-status")]
        [Authorize(Roles = StaticUserRole.DealerManager)]
        public async Task<IActionResult> UpdateStatusDealerStaff([FromQuery] bool isActive, [FromQuery] string applicationUserId, CancellationToken ct)
        {
            var response = await _dealerService.UpdateStatusDealerStaff(User, isActive, applicationUserId, ct);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut]
        [Route("build-daily-inventory-snapshot")]
        public async Task<IActionResult> BuildDailyInventorySnapshot([FromQuery] DateTime utcDate, CancellationToken ct)
        {
            var response = await _dealerForecastService.BuildDailySnapshotAsync(utcDate, ct);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet]
        [Route("get-demand-series")]
        [Authorize]
        public async Task<IActionResult> GetDemandSeries([FromQuery] Guid dealerId, [FromQuery] Guid evTemplateId,
            [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken ct)
        {
            var response = await _dealerForecastService.GetDemandSeriesAsync(User, dealerId, evTemplateId, from, to, ct);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost]
        [Route("upsert-forecast-batch")]
        public async Task<IActionResult> UpsertForecastBatch([FromBody] List<UpsertDealerInventoryForecastDTO> upsertsDTO, CancellationToken ct)
        {
            var response = await _dealerForecastService.UpsertForecastBatchAsync(upsertsDTO, ct);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost]
        [Route("evaluate-inventory-risk/{horizonDays:int}")]
        public async Task<IActionResult> EvaluateInventoryRisk(int horizonDays, CancellationToken ct)
        {
            var response = await _dealerForecastService.EvaluateInventoryRiskAsync(horizonDays, ct);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet]
        [Route("get-forecast-targets")]
        public async Task<IActionResult> GetForecastTargets(CancellationToken ct)
        {
            var response = await _dealerForecastService.GetForecastTargetsAsync(ct);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet]
        [Route("get-forecast-series")]
        [Authorize]
        public async Task<IActionResult> GetForecastSeries([FromQuery] Guid dealerId, [FromQuery] Guid evTemplateId,
            [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken ct)
        {
            var response = await _dealerForecastService.GetForecastSeriesAsync(User, dealerId, evTemplateId, from, to, ct);
            return StatusCode(response.StatusCode, response);
        }
    }
}
