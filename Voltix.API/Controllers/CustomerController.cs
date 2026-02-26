using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.Customer;
using Voltix.Application.IServices;

namespace Voltix.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
        }
        [HttpPost("create-customer")]
        public async Task<ActionResult<ResponseDTO>> CreateCustomer([FromBody] CreateCustomerDTO createCustomerDTO)
        {
            var response = await _customerService.CreateCustomerAsync(User, createCustomerDTO);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-customers-by-id/{customerId}")]
        public async Task<ActionResult<ResponseDTO>> GetCustomersById(Guid customerId)
        {
            var response = await _customerService.GetCustomerByIdAsync(User, customerId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("get-all-customers")]
        public async Task<ActionResult<ResponseDTO>> GetAllCustomers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, string? search = default, CancellationToken ct = default)
        {
            var response = await _customerService.GetAllCustomerAsync(User, pageNumber, pageSize, search, ct);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut("update-customer/{customerId}")]
        public async Task<ActionResult<ResponseDTO>> UpdateCustomerAsync(Guid customerId, [FromBody] UpdateCustomerDTO updateCustomerDTO)
        {
            var response = await _customerService.UpdateCustomerAsync(User, customerId, updateCustomerDTO);
            return StatusCode(response.StatusCode, response);
        }
    }
}
