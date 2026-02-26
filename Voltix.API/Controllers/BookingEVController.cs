using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.BookingEV;
using Voltix.Application.DTO.BookingEVDetail;
using Voltix.Application.IServices;
using Voltix.Domain.Enums;

namespace Voltix.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingEVController : ControllerBase
    {
        public readonly IBookingEVService _bookingEVService;
        public BookingEVController(IBookingEVService bookingEVService)
        {
            _bookingEVService = bookingEVService ?? throw new ArgumentNullException(nameof(bookingEVService));
        }
        [HttpPost("create-booking")]
        public  async Task<ActionResult<ResponseDTO>> CreateBookingEV([FromBody] CreateBookingEVDTO createBookingEVDTO, CancellationToken ct)
        {
            var response = await _bookingEVService.CreateBookingEVAsync(User, createBookingEVDTO, ct);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("get-all-bookings")]
        public async Task<ActionResult<ResponseDTO>> GetAllBookingEVs([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, BookingStatus? bookingStatus = default, CancellationToken ct = default)
        {
            var response = await _bookingEVService.GetAllBookingEVsAsync(User, pageNumber, pageSize, bookingStatus, ct);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("get-booking-by-id/{bookingId}")]
        public async Task<ActionResult<ResponseDTO>> GetBookingEVById([FromRoute] Guid bookingId)
        {
            var response = await _bookingEVService.GetBookingEVByIdAsync(bookingId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut("update-booking-status/{bookingId}")]
        public async Task<ActionResult<ResponseDTO>> UpdateBookingStatus(Guid bookingId, [FromQuery] BookingStatus newStatus)
        {
            var response = await _bookingEVService.UpdateBookingStatusAsync(User, bookingId, newStatus);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("get-vehicles-by-booking-id/{bookingId}")]
        public async Task<ActionResult<ResponseDTO>> GetVehicleByBookingId([FromRoute] Guid bookingId)
        {
            var response = await _bookingEVService.GetVehicleByBookingIdAsync(bookingId);
            return StatusCode(response.StatusCode, response);
        }
    }
}
