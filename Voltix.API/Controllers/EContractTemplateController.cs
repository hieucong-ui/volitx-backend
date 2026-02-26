using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.EContractTemplate;
using Voltix.Application.IServices;

namespace Voltix.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EContractTemplateController : ControllerBase
    {
        private readonly IEContractTemplateService _eContractTemplateService;
        public EContractTemplateController(IEContractTemplateService eContractTemplateService)
        {
            _eContractTemplateService = eContractTemplateService;
        }

        [HttpPost]
        [Route("create-econtract-template")]
        public async Task<ActionResult<ResponseDTO>> CreateEContractTemplateAsync([FromBody] CreateEContractTemplateDTO templateDTO, CancellationToken ct)
        {
            var result = await _eContractTemplateService.CreateEContractTemplateAsync(templateDTO, ct);
            if (result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result.Message);
            }
            return StatusCode(result.StatusCode, result.Message);
        }

        [HttpGet]
        [Route("get-econtract-template-by-id/{eContractId}")]
        public async Task<ActionResult<ResponseDTO>> GetEContractTemplateByEcontractIdAsync([FromRoute] Guid eContractId, CancellationToken ct)
        {
            var result = await _eContractTemplateService.GetEContractTemplateByIdAsync(eContractId, ct);
            if (result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result.Result);
            }
            return StatusCode(result.StatusCode, result.Message);
        }

        [HttpGet]
        [Route("get-all-econtract-template")]
        public async Task<ActionResult<ResponseDTO>> GetAllEContractTemplateAsync([FromQuery] int? pageNumber = 1, [FromQuery] int? pageSize = 10, CancellationToken ct = default)
        {
            var result = await _eContractTemplateService.GetAll(pageNumber, pageSize, ct);
            if (result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result);
            }
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut]
        [Route("update-econtract-template")]
        public async Task<ActionResult<ResponseDTO>> UpdateEContractTemplateAsync([FromQuery] string code, [FromBody] UpdateEContractTemplateDTO templateDTO, CancellationToken ct)
        {
            var result = await _eContractTemplateService.UpdateEcontractTemplateAsync(code, templateDTO, ct);
            return StatusCode(result.StatusCode, result);
        }
    }
}
