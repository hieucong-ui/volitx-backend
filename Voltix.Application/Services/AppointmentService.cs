using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Voltix.Application.DTO.Appointment;
using Voltix.Application.DTO.Auth;
using Voltix.Application.IServices;
using Voltix.Domain.Entities;
using Voltix.Domain.Enums;
using Voltix.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        public readonly IUnitOfWork _unitOfWork;
        public readonly IMapper _mapper;
        public AppointmentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<ResponseDTO> CreateAppointmentAsync(ClaimsPrincipal user, CreateAppointmentDTO createAppointmentDTO, CancellationToken ct)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                    return new ResponseDTO 
                    { 
                        IsSuccess = false, 
                        Message = "User not found", 
                        StatusCode = 404 
                    };

                var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerOrStaffAsync(userId, CancellationToken.None);
                if (dealer == null)
                    return new ResponseDTO 
                    { 
                        IsSuccess = false, 
                        Message = "Dealer not found", 
                        StatusCode = 404 
                    };

                var customer = await _unitOfWork.CustomerRepository.GetByIdAsync(createAppointmentDTO.CustomerId);
                if (customer == null)
                    return new ResponseDTO 
                    { 
                        IsSuccess = false, 
                        Message = "Customer not found", 
                        StatusCode = 404 
                    };

                var evTemplate = await _unitOfWork.EVTemplateRepository.GetByIdAsync(createAppointmentDTO.EVTemplateId);
                if (evTemplate == null)
                    return new ResponseDTO 
                    { IsSuccess = false, 
                        Message = "EV template not found", 
                        StatusCode = 404 
                    };

                var appointmentSetting = await _unitOfWork.DealerConfigurationRepository.GetByDealerIdAsync(dealer.Id, ct);
                if (appointmentSetting == null)
                {
                    appointmentSetting = await _unitOfWork.DealerConfigurationRepository.GetByDefaultAsync(ct);
                    if (appointmentSetting == null)
                    {

                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Appointment setting not found",
                            StatusCode = 404
                        };

                    }
                }
                    if (createAppointmentDTO.StartTime <= DateTime.UtcNow)
                    return new ResponseDTO 
                    { 
                        IsSuccess = false, 
                        Message = "StartTime cannot be in the past", 
                        StatusCode = 400 
                    };

                if (createAppointmentDTO.StartTime >= createAppointmentDTO.EndTime)
                    return new ResponseDTO 
                    { 
                        IsSuccess = false, 
                        Message = "Invalid appointment time range", 
                        StatusCode = 400 
                    };

                if (createAppointmentDTO.StartTime.TimeOfDay < appointmentSetting.OpenTime ||
                    createAppointmentDTO.EndTime.TimeOfDay > appointmentSetting.CloseTime)
                    return new ResponseDTO 
                    { 
                        IsSuccess = false, 
                        Message = "Appointment time is outside dealer working hours", 
                        StatusCode = 400 
                    };

                // Get existing appointments for the dealer on the same day
                var today = createAppointmentDTO.StartTime.Date;
                var appointmentsToday = await _unitOfWork.AppointmentRepository.GetByDealerIdAndDateAsync(dealer.Id, today);

                // Generate available slots based on appointment settings
                var slots = new List<(TimeSpan Start, TimeSpan End)>();
                var currentTime = appointmentSetting.OpenTime;
                var interval = TimeSpan.FromMinutes(appointmentSetting.MinIntervalBetweenAppointments);
                var breakTime = TimeSpan.FromMinutes(appointmentSetting.BreakTimeBetweenAppointments);
                var workEnd = appointmentSetting.CloseTime;

                // Create time slots
                while (currentTime + interval <= workEnd)
                {
                    var slotStart = currentTime;
                    var slotEnd = currentTime + interval;
                    if (slotEnd > workEnd) slotEnd = workEnd;
                    slots.Add((slotStart, slotEnd));
                    currentTime = slotEnd + breakTime;
                }


                // Check if the requested appointment time fits into any of the available slots
                bool isValidSlot = slots.Any(s =>
                    createAppointmentDTO.StartTime.TimeOfDay >= s.Start &&
                    createAppointmentDTO.EndTime.TimeOfDay <= s.End);

                // Validate overlapping appointments
                if (!isValidSlot)
                    return new ResponseDTO 
                    { 
                        IsSuccess = false, 
                        Message = "Appointment time must be within generated slots", 
                        StatusCode = 400 
                    };

                // Check for overlapping appointments
                var overlappingCount = await _unitOfWork.AppointmentRepository.CountOverLappingAsync(
                    dealer.Id,
                    createAppointmentDTO.StartTime,
                    createAppointmentDTO.EndTime);

                // Validate against appointment settings
                if (!appointmentSetting.AllowOverlappingAppointments && overlappingCount >= 1)
                    return new ResponseDTO { IsSuccess = false, Message = "No overlapping allowed, only 1 appointment can be created", StatusCode = 400 };

                if (appointmentSetting.AllowOverlappingAppointments && overlappingCount >= appointmentSetting.MaxConcurrentAppointments)
                    return new ResponseDTO { IsSuccess = false, Message = "Over max concurrent appointments", StatusCode = 400 };


                // Create and save the appointment
                var appointment = new Appointment
                {
                    DealerId = dealer.Id,
                    CustomerId = customer.Id,
                    EVTemplateId = evTemplate.Id,
                    StartTime = createAppointmentDTO.StartTime,
                    EndTime = createAppointmentDTO.EndTime,
                    Status = AppointmentStatus.Active,
                    Note = createAppointmentDTO.Note,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.AppointmentRepository.AddAsync(appointment, CancellationToken.None);
                await _unitOfWork.SaveAsync();

                var getAppointmentDTO = _mapper.Map<GetCreateAppointmentDTO>(appointment);
                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Appointment created successfully",
                    StatusCode = 201,
                    Result = getAppointmentDTO
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500
                };
            }
        }



        public async Task<ResponseDTO> GetAllAppointmentsAsync(ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User not found",
                        StatusCode = 404
                    };
                }

                var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerOrStaffAsync(userId, CancellationToken.None);
                if (dealer == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Dealer not found",
                        StatusCode = 404
                    };
                }

                var appointments = await _unitOfWork.AppointmentRepository.GetAllByDealerIdAsync(dealer.Id);
                if (appointments == null || !appointments.Any())
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "No appointments found",
                        StatusCode = 404
                    };
                }

                var getAppointments = _mapper.Map<List<GetAppointmentDTO>>(appointments);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Appointments retrieved successfully",
                    StatusCode = 200,
                    Result = getAppointments
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500
                };
            }
        }
        public async Task<ResponseDTO> GetAppointmentsByCustomerIdAsync(ClaimsPrincipal user, Guid customerId)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User not found",
                        StatusCode = 404
                    };
                }

                var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerOrStaffAsync(userId, CancellationToken.None);
                if (dealer == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Dealer not found",
                        StatusCode = 404
                    };
                }

                var customer = await _unitOfWork.CustomerRepository.GetByIdAsync(customerId);
                if (customer == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Customer not found",
                        StatusCode = 404
                    };
                }
                // take customer appointments
                var appointments = await _unitOfWork.AppointmentRepository.GetByCustomerIdAsync(customerId);
                if (appointments == null || !appointments.Any())
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "No appointments found",
                        StatusCode = 404
                    };
                }

                var getAppointments = _mapper.Map<List<GetAppointmentDTO>>(appointments);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Appointments retrieved successfully",
                    StatusCode = 200,
                    Result = getAppointments
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> UpdateAppointmentStatusAsync(ClaimsPrincipal user, Guid appointmentId, AppointmentStatus newStatus, CancellationToken ct)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User not found",
                        StatusCode = 404
                    };
                }

                var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerOrStaffAsync(userId, CancellationToken.None);
                if (dealer == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Dealer not found",
                        StatusCode = 404
                    };
                }

                var appointments = await _unitOfWork.AppointmentRepository.GetByIdAsync(appointmentId);
                if (appointments == null || appointments.DealerId != dealer.Id)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "No appointments found for this dealer",
                        StatusCode = 404
                    };
                }

                var appointmentSetting = await _unitOfWork.DealerConfigurationRepository.GetByDealerIdAsync(dealer.Id, ct);
                if (appointmentSetting == null)
                {
                    appointmentSetting = await _unitOfWork.DealerConfigurationRepository.GetByDefaultAsync(ct);
                    if (appointmentSetting == null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Default appointment setting not found",
                            StatusCode = 404
                        };
                    }
                }
                if (newStatus != AppointmentStatus.Completed && newStatus != AppointmentStatus.Canceled)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Invalid status update. Can only update to Complete or Cancelled.",
                        StatusCode = 400
                    };
                }

                if (newStatus == AppointmentStatus.Canceled)
                {
                    if (!appointmentSetting.AllowOverlappingAppointments)
                    {
                        var overlappingCount = await _unitOfWork.AppointmentRepository
                            .CountOverLappingAsync(dealer.Id, appointments.StartTime, appointments.EndTime);
                        if (overlappingCount > 0)
                        {
                            // There are overlapping appointments just minus this one
                        }
                    }

                }

                //if status is completed, no need to check overlapping

                appointments.Status = newStatus;
                _unitOfWork.AppointmentRepository.Update(appointments);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Appointment status updated successfully",
                    StatusCode = 200

                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> UpdateCancelStatusAsync(ClaimsPrincipal user, CancellationToken ct)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User not found",
                        StatusCode = 404
                    };
                }

                var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerOrStaffAsync(userId, ct);
                if (dealer == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Dealer not found",
                        StatusCode = 404
                    };
                }

                var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var todayVN = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone).Date;
                var todayStartUTC = TimeZoneInfo.ConvertTimeToUtc(todayVN, vnTimeZone);

                var appointments = await _unitOfWork.AppointmentRepository.Query(
                    filter: q => q.DealerId == dealer.Id 
                    && q.Status == AppointmentStatus.Active 
                    && q.EndTime < todayStartUTC,
                    includes: null
                ).ToListAsync(ct);

                if (!appointments.Any())
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "No appointments to cancel",
                        StatusCode = 404
                    };
                }
                foreach (var appointment in appointments)
                {
                    appointment.Status = AppointmentStatus.Canceled;
                    _unitOfWork.AppointmentRepository.Update(appointment);
                }
                await _unitOfWork.SaveAsync();
                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Appointments canceled successfully",
                    StatusCode = 200
                };

            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500
                };
            }
        }
    }
}
