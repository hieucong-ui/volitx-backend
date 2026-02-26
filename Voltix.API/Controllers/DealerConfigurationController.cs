using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.DealerConfiguration;
using Voltix.Application.IServices;
using Voltix.Domain.Constants;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Voltix.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DealerConfigurationController : ControllerBase
    {
        private readonly IDealerConfigurationService _dealerConfigurationService;

        public DealerConfigurationController(IDealerConfigurationService dealerConfigurationService)
        {
            _dealerConfigurationService = dealerConfigurationService;
        }

        [HttpGet]
        [Route("get-current")]
        [Authorize]
        public async Task<ActionResult<ResponseDTO<GetDealerConfigurationDTO>>> GetCurrentConfiguration(CancellationToken ct)
        {
            var result = await _dealerConfigurationService.GetCurrentConfigurationAsync(User, ct);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        [Route("upsert-configuration")]
        [Authorize]
        public async Task<ActionResult<ResponseDTO>> UpsertConfiguration([FromBody] UpsertDealerConfigurationDTO dto, CancellationToken ct)
        {
            var result = await _dealerConfigurationService.UpsertConfigurationAsync(User, dto, ct);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut]
        [Route("update-all-deposit-settings")]
        [Authorize(Roles = StaticUserRole.Admin)]
        public async Task<ActionResult<ResponseDTO>> UpdateAllDepositSettings([FromBody] UpdateAllDepositSettingsDTO dto, CancellationToken ct)
        {
            var result = await _dealerConfigurationService.UpdateAllDepositSettingsAsync(User, dto, ct);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet]
        [Route("generate-time-slots")]
        [Authorize]
        public async Task<ActionResult<ResponseDTO>> GenerateTimeSlots([FromQuery] DateTime? targetDate, CancellationToken ct)
        {
            var result = await _dealerConfigurationService.GenerateTimeSlotAsync(User, targetDate, ct);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet]
        [Route("get-default-configuration")]
        public async Task<ActionResult<ResponseDTO>> GetDefaultConfiguration(CancellationToken ct)
        {
            var result = await _dealerConfigurationService.GetDefaultConfigurationAsync(ct);
            return StatusCode(result.StatusCode, result);
        }
    }
}
