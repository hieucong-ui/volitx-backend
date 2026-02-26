using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Voltix.Application.DTO.Auth;
using Voltix.Application.IServices;
using Voltix.Application.Services;
using Voltix.Domain.Enums;

namespace Voltix.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleDeliveryController : ControllerBase
    {
        public readonly IVehicleDeliveryService _vehicleDeliveryService;
        public VehicleDeliveryController(IVehicleDeliveryService vehicleDeliveryService)
        {
            _vehicleDeliveryService = vehicleDeliveryService ?? throw new ArgumentNullException(nameof(vehicleDeliveryService));
        }
        [HttpGet("Get-all-deliveries/")]
        public async Task<ActionResult<ResponseDTO>> GetAllVehicleDeliveries([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, Guid? templateId = default, DeliveryStatus? status = default, bool isShow = default, CancellationToken ct = default)
        {
            var response = await _vehicleDeliveryService.GetAllVehicleDelivery(User, pageNumber, pageSize, status, templateId,isShow ,ct);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("Get-by-id/{deliveryId}")]
        public async Task<ActionResult<ResponseDTO>> GetDeliveryByIdAsync(Guid deliveryId, CancellationToken ct)
        {
            var response = await _vehicleDeliveryService.GetVehicleDeliveryById(deliveryId, ct);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut("update-status/{deliveryId}")]
        public async Task<ActionResult<ResponseDTO>> UpdateDeliveryStatus(
        Guid deliveryId,
        [FromQuery] DeliveryStatus newStatus,
        [FromBody] string? description,
        CancellationToken ct)
        {
            var response = await _vehicleDeliveryService.UpdateVehicleDeliveryStatus(User, deliveryId, newStatus, ct, description);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost("inspect-accident/{deliveryId}")]
        public async Task<ActionResult<ResponseDTO>> InspectAccidentVehicle([FromRoute] Guid deliveryId, [FromBody] List<Guid> damagedVehicleIds, bool isShow , CancellationToken ct)
        {
            var response = await _vehicleDeliveryService.InspectAccidentVehicleAsync(User, deliveryId, damagedVehicleIds,isShow, ct);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost("replace-damaged-vehicle/{deliveryId}")]
        public async Task<ActionResult<ResponseDTO>> ReplaceDamagedVehicle([FromRoute] Guid deliveryId, CancellationToken ct)
        {
            var response = await _vehicleDeliveryService.ReplaceDamagedVehicleAsync(User, deliveryId, ct);
            return StatusCode(response.StatusCode, response);
        }

    }
}
