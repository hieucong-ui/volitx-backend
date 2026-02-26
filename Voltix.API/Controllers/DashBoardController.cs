using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Voltix.Application.DTO.Auth;
using Voltix.Application.IServices;

namespace Voltix.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashBoardController : ControllerBase
    {
        private readonly IDashBoardService _dashBoardService;
        public DashBoardController(IDashBoardService dashBoardService)
        {
            _dashBoardService = dashBoardService;
        }
        //[HttpGet("total-customer")]
        //public async Task<IActionResult> GetTotalCustomerAsync()
        //{
        //    var response = await _dashBoardService.GetTotalCustomerAsync();
        //    return StatusCode(response.StatusCode, response);
        //}
        [HttpGet("get-dealer-manager-dashboard")]
        public async Task<ActionResult<ResponseDTO>> GetDealerDashboard(CancellationToken ct)
        {
            var response = await _dashBoardService.GetDealerDashboardAsync(User, ct);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("get-dealer-revenue-by-quarter")]
        public async Task<ActionResult<ResponseDTO>> GetDealerRevenueByQuarter([FromQuery] int year, CancellationToken ct)
        {
            var response = await _dashBoardService.GetDealerRevenueByQuarterAsync(User, year, ct);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("get-dealer-staff-dashboard")]
        public async Task<ActionResult<ResponseDTO>> GetDealerStaffDashBoard(CancellationToken ct)
        {
            var response = await _dashBoardService.GetDealerStaffDashboardAsync(User, ct);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("get-admin-dashboard")]
        public async Task<ActionResult<ResponseDTO>> GetAdminDashboard(CancellationToken ct)
        {
            var response = await _dashBoardService.GetAdminDashboardAsync(ct);
            return StatusCode(response.StatusCode, response);
        }
    }
}
