using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.DealerFeedBackDTO;
using Voltix.Application.DTO.S3;
using Voltix.Application.IServices;
using Voltix.Domain.Enums;

namespace Voltix.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DealerFeedbackController : ControllerBase
    {
        public readonly IDealerFeedbackService _dealerFeedbackService;
        public readonly IS3Service _s3Service;
        public DealerFeedbackController(IDealerFeedbackService dealerFeedbackService, IS3Service s3Service)
        {
            _dealerFeedbackService = dealerFeedbackService;
            _s3Service = s3Service;
        }
        [HttpPost("CreateDealerFeedback")]
        public async Task<ActionResult<ResponseDTO>> CreateDealerFeedback([FromBody]CreateDealerFeedBackDTO createDealerFeedBackDTO)
        {
            var response = await _dealerFeedbackService.CreateDealerFeedbackAsync(User, createDealerFeedBackDTO);
            return StatusCode(response.StatusCode, response);
            
        }
        [HttpGet("GetAllDealerFeedbacks")]
        public async Task<ActionResult<ResponseDTO>> GetAllDealerFeedbacks(CancellationToken ct)
        {
            var response = await _dealerFeedbackService.GetAllDealerFeedbacksAsync(User,ct);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("GetDealerFeedbackById/{feedbackId}")]
        public async Task<ActionResult<ResponseDTO>> GetDealerFeedbackByIdAsync([FromRoute]Guid feedbackId)
        {
            var response = await _dealerFeedbackService.GetDealerFeedbackByIdAsync(feedbackId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut("Update-dealer-feedback-status/{feedbackId}")]
        public async Task<ActionResult<ResponseDTO>> UpdateDealerFeedbackStatusAsync([FromRoute] Guid feedbackId, FeedbackStatus newStatus)
        {
            var response = await _dealerFeedbackService.UpdateDealerFeedbackStatusAsync(User,feedbackId, newStatus);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost("upload-file-url-dealer-feedback")]
        public ActionResult<ResponseDTO> UploadFileUrlDealerFeedbackAsync([FromBody] PreSignedUploadDTO preSignedUploadDTO)
        {
            var response = _s3Service.GenerateUploadDealerFBAttachment(preSignedUploadDTO);
            return StatusCode(response.StatusCode, response);
        }
    }
}
