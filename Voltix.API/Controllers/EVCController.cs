using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.EVC;
using Voltix.Application.IServices;
using Voltix.Domain.Constants;

namespace Voltix.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EVCController : ControllerBase
    {
        private readonly IEVCService _evcService;
        public EVCController(IEVCService evcService)
        {
            _evcService = evcService ?? throw new ArgumentNullException(nameof(evcService));
        }

        [HttpPost]
        [Route("create-evm-staff")]
        [Authorize(Roles = StaticUserRole.Admin)]
        public async Task<ActionResult<ResponseDTO>> CreateEVMStaff([FromBody] CreateEVMStaffDTO createEVMStaffDTO)
        {
            var response = await _evcService.CreateEVMStaff(createEVMStaffDTO);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet]
        [Route("get-all-evm-staff")]
        //[Authorize(Roles = StaticUserRole.Admin)]
        public async Task<ActionResult<ResponseDTO>> GetAllEVMStaff([FromQuery] string? filterOn, [FromQuery] string? filterQuery, [FromQuery] string? sortBy, [FromQuery] bool? isAcsending, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var response = await _evcService.GetAllEVMStaff(filterOn, filterQuery, sortBy, isAcsending, pageNumber, pageSize);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut]
        [Route("update-evm-staff-status/{evcStaffId}")]
        //[Authorize(Roles = StaticUserRole.Admin)]
        public async Task<ActionResult<ResponseDTO>> UpdateEVCStaffStatus([FromRoute] string evcStaffId, [FromQuery] bool isActive, CancellationToken ct)
        {
            var response = await _evcService.UpdateEVCStaffStatus(evcStaffId, isActive, ct);
            return StatusCode(response.StatusCode, response);
        }
    }
}
