using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Voltix.Application.DTO.Warehouse;
using Voltix.Application.IServices;

namespace Voltix.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarehouseController : ControllerBase
    {
        public readonly IWarehouseService _warehouseService;
        public WarehouseController(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService ?? throw new ArgumentNullException(nameof(warehouseService));
        }
        [HttpPost("create-warehouse")]
        public async Task<ActionResult> CreateWarehouseAsync([FromBody] CreateWarehouseDTO createWarehouseDTO)
        {
            var response = await _warehouseService.CreateWarehouseAsync(createWarehouseDTO);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("get-all-warehouses")]
        public async Task<ActionResult> GetAllWarehousesAsync()
        {
            var response = await _warehouseService.GetAllWarehousesAsync();
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("get-evc-warehouses")]
        public async Task<ActionResult> GetEVCWarehousesAsync()
        {
            var response = await _warehouseService.GetAllEVCWarehouse();
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("get-warehouse-by-id/{warehouseId}")]
        public async Task<ActionResult> GetWarehouseByIdAsync([FromRoute] Guid warehouseId)
        {
            var response = await _warehouseService.GetWarehouseByIdAsync(warehouseId);
            return StatusCode(response.StatusCode, response);
        }
        
    }
}
