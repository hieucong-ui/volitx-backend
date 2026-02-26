using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Voltix.Application.IServices;

namespace Voltix.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DealerDebtController : ControllerBase
    {
        private readonly IDealerDebtService _dealerDebtService;
        public DealerDebtController(IDealerDebtService dealerDebtService)
        {
            _dealerDebtService = dealerDebtService;
        }

        [HttpGet]
        [Route("get-balance-at-quarter-now")]
        public async Task<IActionResult> GetDealerDebtBalanceAtQuarterNow([FromQuery] Guid? dealerId, CancellationToken ct)
        {
            var result = await _dealerDebtService.GetDealerDebtBalanceAtQuarterNow(User, dealerId, ct);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet]
        [Route("get-debt-details")]
        public async Task<IActionResult> GetDealerDebtDetails([FromQuery] Guid? dealerId, [FromQuery] DateTime fromDateUtc, [FromQuery] DateTime toDateUtc, [FromQuery] int pageNumber, [FromQuery] int pageSize, CancellationToken ct)
        {
            var result = await _dealerDebtService.GetDealerDebtDetails(User, dealerId, fromDateUtc, toDateUtc, pageNumber, pageSize, ct);
            return StatusCode(result.StatusCode, result);
        }
    }
}
