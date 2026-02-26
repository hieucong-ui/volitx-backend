using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Voltix.Application.DTO.Auth;
using Voltix.Application.IServices;

namespace Voltix.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        [Route("get-all-notification")]
        public async Task<ActionResult<ResponseDTO>> GetAllNotification([FromQuery] int pageNumber, [FromQuery] int pageSize, CancellationToken ct)
        {
            var result = await _notificationService.GetAllNotification(User, pageNumber, pageSize, ct);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        [Route("read-notification")]
        public async Task<ActionResult<ResponseDTO>> ReadNotification([FromQuery] Guid notificationId, CancellationToken ct)
        {
            var result = await _notificationService.ReadNotification(notificationId, ct);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        [Route("read-all-notification")]
        public async Task<ActionResult<ResponseDTO>> ReadAllNotification(CancellationToken ct)
        {
            var result = await _notificationService.RealAll(User, ct);
            return StatusCode(result.StatusCode, result);
        }
    }
}
