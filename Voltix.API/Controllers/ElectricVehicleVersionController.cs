using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.ElectricVehicleVersion;
using Voltix.Application.IServices;
using Voltix.Domain.Enums;

namespace Voltix.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ElectricVehicleVersionController : ControllerBase
    {
        public readonly IElectricVehicleVersionService _electricVehicleVersionService;
        public ElectricVehicleVersionController(IElectricVehicleVersionService electricVehicleVersionService)
        {
            _electricVehicleVersionService = electricVehicleVersionService ?? throw new ArgumentNullException(nameof(electricVehicleVersionService));
        }
        [HttpPost("create-version")]
        public async Task<ActionResult<ResponseDTO>> CreateVersionAsync([FromBody] CreateElectricVehicleVersionDTO createElectricVehicleVersionDTO)
        {
            var response = await _electricVehicleVersionService.CreateVersionAsync(createElectricVehicleVersionDTO);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("get-all-versions")]
        public async Task<ActionResult<ResponseDTO>> GetAllVersionsByModelIdAsync()
        {
            var response = await _electricVehicleVersionService.GetAllVersionsAsync();
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("get-version-by-id/{versionId}")]
        public async Task<ActionResult<ResponseDTO>> GetVersionByIdAsync([FromRoute] Guid versionId)
        {
            var response = await _electricVehicleVersionService.GetVersionByIdAsync(versionId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut("update-version/{versionId}")]
        public async Task<ActionResult<ResponseDTO>> UpdateVersionAsync(Guid versionId, [FromBody] UpdateElectricVehicleVersionDTO updateElectricVehicleVersionDTO)
        {
            var response = await _electricVehicleVersionService.UpdateVersionAsync(versionId, updateElectricVehicleVersionDTO);
            return StatusCode(response.StatusCode, response);
        }
        
        [HttpGet("get-all-available-versions-for-booking-by-model-id/{modelId}")]
        public async Task<ActionResult<ResponseDTO>> GetAllAvailableVersionsForBookingByModelIdAsync([FromRoute] Guid modelId)
        {
            var response = await _electricVehicleVersionService.GetAllAvailableVersionsForBookingByModelIdAsync(modelId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("get-all-available-versions-by-model-id/{modelId}")]
        public async Task<ActionResult<ResponseDTO>> GetAllAvailableVersionsByModelIdAsync([FromRoute] Guid modelId)
        {
            var response = await _electricVehicleVersionService.GetAllAvailableVersionsByModelIdAsync(modelId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpDelete("detele-version-by-id/{versionId}")]
        public async Task<ActionResult<ResponseDTO>> DeleteVersionByIdAsync([FromRoute] Guid versionId)
        {
            var response = await _electricVehicleVersionService.DeleteVersionAsync(versionId);
            return StatusCode(response.StatusCode, response);
        }

    }
}
