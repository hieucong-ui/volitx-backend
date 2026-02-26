using Voltix.Application.DTO.Appointment;
using Voltix.Application.DTO.Auth;
using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.IServices
{
    public interface IAppointmentService
    {
        Task<ResponseDTO> CreateAppointmentAsync(ClaimsPrincipal user, CreateAppointmentDTO createAppointmentDTO, CancellationToken ct);
        Task<ResponseDTO> GetAllAppointmentsAsync(ClaimsPrincipal user);
        Task<ResponseDTO> GetAppointmentsByCustomerIdAsync(ClaimsPrincipal user, Guid customerId);
        Task<ResponseDTO> UpdateAppointmentStatusAsync(ClaimsPrincipal user, Guid appointmentId, AppointmentStatus newStatus, CancellationToken ct);
        Task<ResponseDTO> UpdateCancelStatusAsync(ClaimsPrincipal user, CancellationToken ct);
    }
}
