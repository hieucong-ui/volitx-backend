using EVManagementSystem.Application.DTO.EContract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.EContract;
using Voltix.Application.DTO.S3;
using Voltix.Application.IServices;
using Voltix.Application.Pdf;
using Voltix.Domain.Constants;
using Voltix.Domain.Enums;
using Voltix.Domain.ValueObjects;
using System.Text;

namespace Voltix.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EContractController : ControllerBase
    {
        private readonly IEContractService _econtractService;
        private readonly IS3Service _s3Service;
        public EContractController(IEContractService econtractService, IS3Service s3Service)
        {
            _econtractService = econtractService;
            _s3Service = s3Service;
        }


        [HttpGet]
        [Route("get-info-to-sign-process-by-code")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDTO>> GetInfoSignProcess([FromQuery] string processCode)
        {
            var r = await _econtractService.GetAccessTokenAsyncByCode(processCode);
            return Ok(r);
        }

        [HttpGet]
        [Route("get-access-token-for-evc")]
        //[Authorize(Roles = StaticUserRole.Admin)]
        public async Task<ActionResult<ResponseDTO>> GetAccessToken()
        {
            var r = await _econtractService.GetAccessTokenAsync();
            return StatusCode(r.StatusCode, r);
        }

        [HttpPost]
        [Route("ready-dealer-contracts")]
        //[Authorize(Roles = StaticUserRole.Admin_EVMStaff)]
        public async Task<ActionResult<ResponseDTO>> CreateEContractAsync([FromQuery] Guid eContractId, CancellationToken ct)
        {
            var r = await _econtractService.CreateEContractAsync(User, eContractId, ct);
            return StatusCode(r.StatusCode, r);
        }

        [HttpPost]
        [Route("draft-dealer-contracts")]
        //[Authorize(Roles = StaticUserRole.Admin_EVMStaff)]
        public async Task<ActionResult<ResponseDTO>> CreateDraftDealerContract([FromBody] CreateDealerDTO dto, CancellationToken ct)
        {
            var r = await _econtractService.CreateDraftEContractAsync(User, dto, ct);
            return StatusCode(r.StatusCode, r);
        }

        // Sign process
        [HttpPost]
        [Route("sign-process")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDTO>> SignProcess([FromQuery] string token, [FromBody] VnptProcessDTO dto, CancellationToken ct)
        {
            var r = await _econtractService.SignProcess(token, dto, ct);
            return StatusCode(r.StatusCode, r);
        }


        [HttpGet]
        [Route("preview")]
        [AllowAnonymous]
        public async Task<IActionResult> Preview([FromQuery] string downloadURL, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(downloadURL))
                return BadRequest("Missing downloadURL");

            Request.Headers.TryGetValue("Range", out var rangeHeader);

            var upstream = await _econtractService.GetPreviewResponseAsync(downloadURL, rangeHeader.ToString(), ct);
            HttpContext.Response.RegisterForDispose(upstream);

            if (!upstream.IsSuccessStatusCode)
            {
                var err = await upstream.Content.ReadAsStringAsync(ct);
                return StatusCode((int)upstream.StatusCode, err);
            }

            var stream = await upstream.Content.ReadAsStreamAsync(ct);
            var contentType = upstream.Content.Headers.ContentType?.ToString() ?? "application/pdf";

            Response.StatusCode = (int)upstream.StatusCode;

            if (upstream.Headers.AcceptRanges is { Count: > 0 })
                Response.Headers["Accept-Ranges"] = string.Join(",", upstream.Headers.AcceptRanges);

            if (upstream.Content.Headers.ContentRange is not null)
                Response.Headers["Content-Range"] = upstream.Content.Headers.ContentRange.ToString();

            if (upstream.Content.Headers.ContentLength is long len)
                Response.ContentLength = len;

            var upstreamCd = upstream.Content.Headers.ContentDisposition;
            var safeFileName = upstreamCd?.FileNameStar ?? upstreamCd?.FileName ?? "document.pdf";

            var cd = new ContentDispositionHeaderValue("inline")
            {
                FileNameStar = safeFileName
            };
            Response.Headers[HeaderNames.ContentDisposition] = cd.ToString();

            Response.ContentType = contentType;
            Response.Headers[HeaderNames.CacheControl] = "no-store";

            return new FileStreamResult(stream, contentType);
        }

        [HttpPost]
        [Route("add-smartca")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDTO>> AddSmartCA([FromBody] AddNewSmartCADTO dto)
        {
            var r = await _econtractService.AddSmartCA(dto);
            return Ok(r);
        }

        [HttpGet]
        [Route("smartca-info/{userId}")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDTO>> GetSmartCAInformation([FromRoute] int userId)
        {
            var r = await _econtractService.GetSmartCAInformation(userId);
            return Ok(r);
        }

        [HttpPost]
        [Route("update-smartca")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDTO>> UpdateSmartCA([FromBody] UpdateSmartDTO dto)
        {
            var r = await _econtractService.UpdateSmartCA(dto);
            return Ok(r);
        }

        [HttpPost]
        [Route("delete-smartca")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDTO>> DeleteSmartCA([FromBody] DeleteSmartCARequest dto)
        {
            var r = await _econtractService.DeleteSmartCA(dto);
            return Ok(r);
        }

        [HttpPost]
        [Route("update-econtract")]
        [Consumes("application/json")]
        [Authorize(Roles = StaticUserRole.AllRolesInSystem)]
        public async Task<ActionResult<ResponseDTO>> UpdateEContract([FromBody] UpdateEContractDTO dto, CancellationToken ct)
        {
            var r = await _econtractService.UpdateEContract(User, dto, ct);
            return Ok(r);
        }

        [HttpGet]
        [Route("get-all-econtract-list")]
        //[Authorize(Roles = StaticUserRole.Admin_EVMStaff)]
        public async Task<ActionResult<ResponseDTO>> GetEContractList([FromQuery] int? pageNumber = 1, [FromQuery] int? pageSize = 10, [FromQuery] EContractStatus eContractStatus = default,
            [FromQuery] EcontractType econtractType = default, CancellationToken ct = default)
        {
            var r = await _econtractService.GetAllEContractList(User, pageNumber, pageSize, eContractStatus, econtractType, ct);
            return Ok(r);
        }

        [HttpPost]
        [Route("get-all-vnpt-econtract")]
        public async Task<ActionResult<ResponseDTO>> GetVnptEContractByIdPost([FromQuery] int? pageNumber = 1, [FromQuery] int? pageSize = 10, [FromQuery] EContractStatus eContractStatus = default)
        {
            var r = await _econtractService.GetAllVnptEContractList(pageNumber, pageSize, eContractStatus);
            return Ok(r);
        }

        [HttpGet]
        [Route("get-vnpt-econtract-by-id/{eContractId}")]
        //[Authorize(Roles = StaticUserRole.Admin_EVMStaff)]
        public async Task<ActionResult<ResponseDTO>> GetVnptEContractById([FromRoute] string eContractId, CancellationToken ct)
        {
            var r = await _econtractService.GetVnptEContractByIdAsync(eContractId, ct);
            return Ok(r);
        }

        [HttpGet]
        [Route("get-econtract-by-id/{eContractId}")]
        //[Authorize(Roles = StaticUserRole.Admin_EVMStaff)]
        public async Task<ActionResult<ResponseDTO>> GetEContractById([FromRoute] string eContractId, CancellationToken ct)
        {
            var r = await _econtractService.GetEContractByIdAsync(eContractId, ct);
            return StatusCode((int)r.StatusCode, r);
        }

        [HttpDelete]
        [Route("delete-econtract-draft/{econtractId}")]
        public async Task<ActionResult<ResponseDTO>> DeleteEContractDraft([FromRoute] Guid econtractId, CancellationToken ct)
        {
            var r = await _econtractService.DeleteEContractDraft(econtractId, ct);
            return StatusCode(r.StatusCode, r);
        }

        [HttpPost]
        [Route("ready-customerorder-econtract")]
        public async Task<ActionResult<ResponseDTO>> ReadyCustomerOrderEcontract([FromQuery] Guid eContractId, CancellationToken ct)
        {
            var r = await _econtractService.ReadyCustomerOrderEcontract(eContractId, ct);
            return StatusCode(r.StatusCode, r);
        }

        [HttpPost]
        [Route("confirm-booking-econtract")]
        public async Task<ActionResult<ResponseDTO>> ConfirmBookingEVEContract([FromQuery] Guid EContractId, CancellationToken ct)
        {
            var r = await _econtractService.ConfirmBookingEVEContract(User, EContractId, ct);
            return StatusCode(r.StatusCode, r);
        }

        [HttpPost]
        [Route("confirm-vin-bookingev")]
        public async Task<ActionResult<ResponseDTO>> CreateEContractInvoiceConfirmBookingEV([FromQuery] Guid customerOrderId, CancellationToken ct)
        {
            var r = await _econtractService.CreateEContractInvoiceConfirmBookingEV(customerOrderId, ct);
            return StatusCode(r.StatusCode, r);
        }
    }
}
