using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.Payment;
using Voltix.Application.IServices;
using Voltix.Domain.Enums;

namespace Voltix.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost]
        [Route("create-vnpay/{customerOrderId:guid}")]
        public async Task<ActionResult<ResponseDTO>> CreateVNPayLink(Guid customerOrderId, CancellationToken ct)
        {
            var response = await _paymentService.CreateVNPayLink(customerOrderId, ct);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet]
        [Route("IPN")]
        public async Task<IActionResult> VNPayIPN([FromQuery] VNPayIPNDTO ipnDTO, CancellationToken ct)
        {
            var response = await _paymentService.HandleVNPayIpn(ipnDTO, ct);

            if (response.Result is VNPayIpnResponse ok)
            {
                return new JsonResult(ok);
            }

            return new JsonResult(new VNPayIpnResponse("99", "Unknown error"));
        }

        [HttpPost]
        [Route("get-all-transactions")]
        public async Task<ActionResult<ResponseDTO>> GetAllTransactions([FromQuery] int pageNumber, [FromQuery] int pageSize, [FromQuery] TransactionStatus? status, CancellationToken ct)
        {
            var response = await _paymentService.GetAllPaymentTransaction(User, pageNumber, pageSize, status, ct);
            return StatusCode(response.StatusCode, response);
        }
    }
}
