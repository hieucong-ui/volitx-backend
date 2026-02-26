using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.Quote;
using Voltix.Application.IServices;
using Voltix.Application.Services;
using Voltix.Domain.Enums;
using System.Security.Claims;

namespace Voltix.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuoteController : ControllerBase
    {
        public readonly IQuoteService _quoteService;
        public QuoteController(IQuoteService quoteService)
        {
            _quoteService = quoteService ?? throw new ArgumentNullException(nameof(quoteService));
        }
        [HttpPost("create-quote")]
        public async Task<ActionResult<ResponseDTO>> CreateQuoteAsync([FromBody] CreateQuoteDTO createQuoteDTO)
        {
            var response = await _quoteService.CreateQuoteAsync(User, createQuoteDTO);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("get-all-quote")]
        public async Task<ActionResult<ResponseDTO>> GetAllQuoteAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, Guid? modelId = null,Guid? versionId = null,Guid? colorId = null, QuoteStatus? status = default, bool onlyToday = false, CancellationToken ct = default)
        {
            var response = await _quoteService.GetAllAsync(User,pageNumber,pageSize,modelId,versionId,colorId,status,onlyToday,ct);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("get-quote-by-id/{quoteId}")]
        public async Task<ActionResult<ResponseDTO>> GetQuoteByIdAsync([FromRoute] Guid quoteId)
        {
            var response = await _quoteService.GetQuoteByIdAsync(User, quoteId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("Update-quote-status/{quoteId}")]
        public async Task<ActionResult<ResponseDTO>> UpdateQuoteStatusAsync(Guid quoteId , [FromQuery] QuoteStatus newStatus)
        {
            var response = await _quoteService.UpdateQuoteStatusAsync(User, quoteId, newStatus);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut("Update-expired-quote")]
        public async Task<ActionResult<ResponseDTO>> UpdateExpiredQuoteAsync(CancellationToken ct = default)
        {
            var response = await _quoteService.UpdateExpiredQuoteAsync(User, ct);
            return StatusCode(response.StatusCode, response);
        }
    }
}
