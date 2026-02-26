using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.CustomerFeedback;
using Voltix.Application.DTO.S3;
using Voltix.Application.IServices;

namespace Voltix.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerFeedbackController : ControllerBase
    {
        public ICustomerFeedbackService _customerFeedbackService;
        public IS3Service _s3Service;
        public CustomerFeedbackController(ICustomerFeedbackService customerFeedbackService, IS3Service s3Service)
        {
            _customerFeedbackService = customerFeedbackService;
            _s3Service = s3Service;
        }

        [HttpPost("CreateCustomerFeedback")]
        public async Task<ActionResult<ResponseDTO>> CreateCustomerFeedback([FromBody] CreateCustomerFeedbackDTO createCustomerFeedbackDTO)
        {
            var response = await _customerFeedbackService.CreateCustomerFeedbackAsync(User, createCustomerFeedbackDTO);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("GetAllCustomerFeedbacks")]
        public async Task<ActionResult<ResponseDTO>> GetAllCustomerFeedbacks(CancellationToken ct)
        {
            var response = await _customerFeedbackService.GetAllCustomerFeedbacksAsync(User, ct);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("GetCustomerFeedbackById/{feedbackId}")]
        public async Task<ActionResult<ResponseDTO>> GetCustomerFeedbackByIdAsync([FromRoute] Guid feedbackId)
        {
            var response = await _customerFeedbackService.GetCustomerFeedbackByIdAsync(User, feedbackId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost("upload-file-url-customer-feedback")]
        public ActionResult<ResponseDTO> UploadFileUrlCustomerFeedbackAsync([FromBody] PreSignedUploadDTO preSignedUploadDTO)
        {
            var response = _s3Service.GenerateUploadCustomerFBAttachment(preSignedUploadDTO);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut("update-customer-feedback-status/{feedbackId}")]
        public async Task<ActionResult<ResponseDTO>> UpdateCustomerFeedbackStatusAsync([FromRoute] Guid feedbackId, Domain.Enums.FeedbackStatus newStatus)
        {
            var response = await _customerFeedbackService.UpdateCustomerFeedbackStatusAsync(User, feedbackId, newStatus);
            return StatusCode(response.StatusCode, response);
        }
    }
}
