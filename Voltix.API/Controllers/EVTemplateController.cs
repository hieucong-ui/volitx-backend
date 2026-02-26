using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.EVTemplate;
using Voltix.Application.IServices;
using System.Threading.Tasks;

namespace Voltix.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EVTemplateController : ControllerBase
    {
        public readonly IEVTemplateService _evTemplateService;

        public EVTemplateController(IEVTemplateService evTemplateService)
        {
            _evTemplateService = evTemplateService ?? throw new ArgumentNullException(nameof(evTemplateService));
        }

        [HttpPost("create-template-vehicles")]
        public async Task<ActionResult<ResponseDTO>> CreateEVTemplateAsync(CreateEVTemplateDTO createEVTemplateDTO)
        {
            var response = await _evTemplateService.CreateEVTemplateAsync(createEVTemplateDTO);
            return StatusCode(response.StatusCode,response);
        }

        [HttpGet("Get-all-template-vehicles")]
        public async Task<IActionResult> GetAllVehicleTemplates(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] Guid? templateId = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] bool sortByPriceAsc = true,
        CancellationToken ct = default)
        {
            var response = await _evTemplateService.GetAllVehicleTemplateAsync(pageNumber, pageSize, search, templateId,minPrice,maxPrice,sortByPriceAsc , ct);
            return StatusCode(response.StatusCode,response);
        }

        [HttpGet("get-template-by-id/{EVTemplateId}")]
        public async Task<ActionResult<ResponseDTO>> GetEVTemplateById(Guid EVTemplateId)
        {
            var response = await _evTemplateService.GetVehicleTemplateByIdAsync(EVTemplateId);
            return StatusCode(response.StatusCode,response);
        }

        [HttpPut("update-template-vehicle/{EVTemplateId}")]
        public async Task<ActionResult<ResponseDTO>> UpdateEVTemplate(Guid EVTemplateId ,UpdateEVTemplateDTO updateEVTemplateDTO)
        {
            var response = await _evTemplateService.UpdateEVTemplateAsync(EVTemplateId, updateEVTemplateDTO);
            return StatusCode(response.StatusCode,response);
        }

        [HttpDelete("delete-template/{EVTemplateId}")]
        public async Task<ActionResult<ResponseDTO>> DeleteEVTemplate(Guid EVTemplateId)
        {
            var response = await _evTemplateService.DeleteEVTemplateAsync(EVTemplateId);
            return StatusCode(response.StatusCode,response);
        }

        [HttpGet("get-template-by-version-and-color/{versionId}/{colorId}")]
        public async Task<ActionResult<ResponseDTO>> GetTemplesByVersionAndColor(Guid versionId, Guid colorId)
        {
            var response = await _evTemplateService.GetTemplatesByVersionAndColorAsync(versionId, colorId);
            return StatusCode(response.StatusCode,response);
        }
    }
}
