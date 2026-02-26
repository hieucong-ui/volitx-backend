using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.ElectricVehicle;
using Voltix.Application.DTO.S3;
using Voltix.Application.IServices;
using Voltix.Application.Services;
using Voltix.Domain.Enums;
using System.Threading.Tasks;

namespace Voltix.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ElectricVehicleController : ControllerBase
    {
        public readonly IElectricVehicleService _electricVehicleService;
        private readonly IS3Service _s3Service;
        public ElectricVehicleController(IElectricVehicleService electricVehicleService, IS3Service s3Service)
        {
            _electricVehicleService = electricVehicleService ?? throw new ArgumentNullException(nameof(electricVehicleService));
            _s3Service = s3Service;

        }
        [HttpPost("create-vehicle")]
        public async Task<ActionResult<ResponseDTO>> CreateElectricVehicleAsync([FromBody] CreateElecticVehicleDTO createElecticVehicleDTO)
        {
            var response = await _electricVehicleService.CreateVehicleAsync(createElecticVehicleDTO);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("get-all-vehicles")]
        public async Task<ActionResult> GetAllVehiclesAsync()
        {
            var response = await _electricVehicleService.GetAllVehiclesAsync(User);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("get-vehicle-by-id/{vehicleId}")]
        public async Task<ActionResult<ResponseDTO>> GetVehicleByIdAsync([FromRoute] Guid vehicleId)
        {
            var response = await _electricVehicleService.GetVehicleByIdAsync(vehicleId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("get-vehicle-by-vin/{vin}")]
        public async Task<ActionResult<ResponseDTO>> GetVehicleByVINAsync([FromRoute] string vin)
        {
            var response = await _electricVehicleService.GetVehicleByVinAsync(vin);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("get-available-quantity-by-model-version-color/{modelId}/{versionId}/{colorId}")]
        public async Task<ActionResult<ResponseDTO>> GetAvailableQuantityByModelVersionColor([FromRoute] Guid modelId, [FromRoute] Guid versionId, [FromRoute] Guid colorId)
        {
            var response = await _electricVehicleService.GetAvailableQuantityByModelVersionColorAsync(modelId, versionId, colorId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("update-vehicle")]
        public async Task<ActionResult<ResponseDTO>> UpdateVehicleAsync(Guid vehicleId, [FromBody] UpdateElectricVehicleDTO updateElectricVehicleDTO)
        {
            var response = await _electricVehicleService.UpdateVehicleAsync(vehicleId, updateElectricVehicleDTO);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut("update-vehicle-status/{vehicleId}")]
        public async Task<ActionResult<ResponseDTO>> UpdateVehicleStatusAsync([FromRoute] Guid vehicleId, [FromQuery] ElectricVehicleStatus status)
        {
            var response = await _electricVehicleService.UpdateVehicleStatusAsync(vehicleId, status);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost]
        [Route("upload-file-url-electric-vehicle")]
        public ActionResult<ResponseDTO> GenerateUploadElectricVehicle([FromBody] PreSignedUploadDTO preSignedUploadDTO)
        {
            var response = _s3Service.GenerateUploadElectricVehicle(preSignedUploadDTO);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("get-dealer-inventory")]
        public async Task<ActionResult<ResponseDTO>> GetDealerInventoryAsync()
        {
            var response = await _electricVehicleService.GetDealerInventoryAsync(User);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("get-evc-inventory")]
        public async Task<ActionResult<ResponseDTO>> GetEVCInventoryAsync(int pageNumber = 1, int pageSize = 10, Guid? warehouseId = null, CancellationToken ct = default)
        {
            var response = await _electricVehicleService.GetEVCInventoryAsync(User, pageNumber, pageSize, warehouseId, ct);
            return StatusCode(response.StatusCode, response);
        }
    }
}
