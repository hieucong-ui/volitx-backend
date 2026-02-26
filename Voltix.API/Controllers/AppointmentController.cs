using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Voltix.Application.DTO.Appointment;
using Voltix.Application.DTO.Auth;
using Voltix.Application.IServices;
using Voltix.Domain.Enums;

namespace Voltix.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        public readonly IAppointmentService _appointmentService;
        public AppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService ?? throw new ArgumentNullException(nameof(appointmentService));
        }

        [HttpPost("create-appointment")]
        public async Task<ActionResult<ResponseDTO>> CreateAppointmentAsync([FromBody] CreateAppointmentDTO createAppointmentDTO, CancellationToken ct)
        {
            var response = await _appointmentService.CreateAppointmentAsync(User, createAppointmentDTO, ct);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("get-all-appointment")]
        public async Task<ActionResult<ResponseDTO>> GetAllAppointmentsAsync()
        {
            var response = await _appointmentService.GetAllAppointmentsAsync(User);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("get-appointment-by-customer-id/{customerId}")]
        public async Task<ActionResult<ResponseDTO>> GetAppointmentByCustomerIdAsync([FromRoute] Guid customerId)
        {
            var response = await _appointmentService.GetAppointmentsByCustomerIdAsync(User, customerId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut("update-appointment-by-id/{appointmentId}")]
        public async Task<ActionResult<ResponseDTO>> UpdateAppointmentAsync([FromRoute] Guid appointmentId, AppointmentStatus newStatus, CancellationToken ct)
        {
            var response = await _appointmentService.UpdateAppointmentStatusAsync(User, appointmentId, newStatus, ct);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut("update-cancel-appointment")]
        public async Task<ActionResult<ResponseDTO>> UpdateCancelStatusAsync(CancellationToken ct)
        {
            var response = await _appointmentService.UpdateCancelStatusAsync(User, ct);
            return StatusCode(response.StatusCode, response);
        }
    }
}
