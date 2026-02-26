using AutoMapper;
using Voltix.Application.DTO;
using Voltix.Application.DTO.AppointmentSetting;
using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.DealerConfiguration;
using Voltix.Application.IServices;
using Voltix.Domain.Constants;
using Voltix.Domain.Entities;
using Voltix.Domain.Enums;
using Voltix.Infrastructure.IRepository;
using System.Security.Claims;

namespace Voltix.Application.Services
{
    public class DealerConfigurationService : IDealerConfigurationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DealerConfigurationService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        #region Helper

        private static string? GetUserId(ClaimsPrincipal userClaim)
            => userClaim.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        private static string? GetUserRole(ClaimsPrincipal userClaim)
            => userClaim.FindFirst(ClaimTypes.Role)?.Value;

        private static ResponseDTO Error(int statusCode, string message) =>
            new ResponseDTO
            {
                IsSuccess = false,
                StatusCode = statusCode,
                Message = message
            };

        private static ResponseDTO<T> Error<T>(int statusCode, string message) =>
            new ResponseDTO<T>
            {
                IsSuccess = false,
                StatusCode = statusCode,
                Message = message
            };

        private static bool IsValidTimeRange(TimeSpan? open, TimeSpan? close, out string? error)
        {
            error = null;
            if (open.HasValue && close.HasValue)
            {
                if (open.Value >= close.Value)
                {
                    error = "Open time must be earlier than close time.";
                    return false;
                }

                var totalHours = (close.Value - open.Value).TotalHours;
                if (totalHours <= 0 || totalHours >= 24)
                {
                    error = "Total working hours must be between 0 and 24 hours.";
                    return false;
                }
            }

            return true;
        }

        private static bool IsValidDepositRange(decimal? min, decimal? max, out string? error)
        {
            error = null;

            if (min.HasValue && (min.Value < 0 || min.Value > 100))
            {
                error = "Minimum deposit percentage must be between 0 and 100.";
                return false;
            }

            if (max.HasValue && (max.Value < 0 || max.Value > 100))
            {
                error = "Maximum deposit percentage must be between 0 and 100.";
                return false;
            }

            if (min.HasValue && max.HasValue && min.Value > max.Value)
            {
                error = "Min deposit percentage cannot be greater than max deposit percentage.";
                return false;
            }

            return true;
        }

        #endregion

        #region Get current configuration

        public async Task<ResponseDTO<GetDealerConfigurationDTO>> GetCurrentConfigurationAsync(ClaimsPrincipal userClaim, CancellationToken ct)
        {
            try
            {
                var userId = GetUserId(userClaim);
                if (userId is null)
                    return Error<GetDealerConfigurationDTO>(401, "User not login yet.");

                var role = GetUserRole(userClaim);
                if (role is null)
                    return Error<GetDealerConfigurationDTO>(403, "User role not found.");

                DealerConfiguration? config;

                if (role.Equals(StaticUserRole.Admin))
                {
                    config = await _unitOfWork.DealerConfigurationRepository.GetByUserIdAsync(userId, ct)
                             ?? await _unitOfWork.DealerConfigurationRepository.GetByDefaultAsync(ct);
                }
                else
                {
                    var dealer = await _unitOfWork.DealerRepository.GetTrackedDealerByManagerOrStaffAsync(userId, ct);

                    if (dealer is null)
                        return Error<GetDealerConfigurationDTO>(404, "Dealer not found for this user.");

                    config = await _unitOfWork.DealerConfigurationRepository.GetByDealerIdAsync(dealer.Id, ct)
                             ?? await _unitOfWork.DealerConfigurationRepository.GetByDefaultAsync(ct);
                }

                if (config is null)
                    return Error<GetDealerConfigurationDTO>(404, "Dealer configuration not found.");

                var dto = _mapper.Map<GetDealerConfigurationDTO>(config);

                return new ResponseDTO<GetDealerConfigurationDTO>
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Dealer configuration retrieved successfully.",
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO<GetDealerConfigurationDTO>
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"An error occurred when getting dealer configuration: {ex.Message}"
                };
            }
        }

        #endregion

        #region Upsert (create/update) configuration

        public async Task<ResponseDTO> UpsertConfigurationAsync(ClaimsPrincipal userClaim, UpsertDealerConfigurationDTO dto, CancellationToken ct)
        {
            try
            {
                var userId = GetUserId(userClaim);
                if (userId is null)
                    return Error(401, "User not login yet.");

                var role = GetUserRole(userClaim);
                if (role is null)
                    return Error(403, "User role not found.");

                // Validate thời gian
                if (!IsValidTimeRange(dto.OpenTime, dto.CloseTime, out var timeError))
                    return Error(400, timeError!);

                // Validate deposit
                if (!IsValidDepositRange(dto.MinDepositPercentage, dto.MaxDepositPercentage, out var depositError))
                    return Error(400, depositError!);

                DealerConfiguration? config;
                Dealer? dealer = null;

                if (role.Equals(StaticUserRole.Admin))
                {
                    // Config default cho Admin (DealerId = null)
                    config = await _unitOfWork.DealerConfigurationRepository.GetByUserIdAsync(userId, ct)
                             ?? await _unitOfWork.DealerConfigurationRepository.GetByDefaultAsync(ct);

                    if (config is null)
                    {
                        config = new DealerConfiguration
                        {
                            ManagerId = userId,
                            DealerId = null
                        };

                        await _unitOfWork.DealerConfigurationRepository.AddAsync(config, ct);
                    }
                }
                else if (role.Equals(StaticUserRole.DealerManager))
                {
                    dealer = await _unitOfWork.DealerRepository.GetDealerByManagerIdAsync(userId, ct);
                    if (dealer is null)
                        return Error(404, "Dealer not found for the manager.");

                    config = await _unitOfWork.DealerConfigurationRepository.GetByDealerIdAsync(dealer.Id, ct);
                    if (config is null)
                    {
                        var defaultConfig = await _unitOfWork.DealerConfigurationRepository.GetByDefaultAsync(ct);
                        config = new DealerConfiguration
                        {
                            ManagerId = userId,
                            DealerId = dealer.Id,
                            AllowOverlappingAppointments = defaultConfig?.AllowOverlappingAppointments ?? false,
                            MaxConcurrentAppointments = defaultConfig?.MaxConcurrentAppointments ?? 1,
                            OpenTime = defaultConfig?.OpenTime ?? TimeSpan.FromHours(9),
                            CloseTime = defaultConfig?.CloseTime ?? TimeSpan.FromHours(17),
                            MinIntervalBetweenAppointments = defaultConfig?.MinIntervalBetweenAppointments ?? 30,
                            BreakTimeBetweenAppointments = defaultConfig?.BreakTimeBetweenAppointments ?? 0,
                            MinDepositPercentage = defaultConfig?.MinDepositPercentage ?? 0,
                            MaxDepositPercentage = defaultConfig?.MaxDepositPercentage ?? 20
                        };

                        await _unitOfWork.DealerConfigurationRepository.AddAsync(config, ct);
                        await _unitOfWork.SaveAsync();
                    }
                }
                else
                {
                    return Error(403, "Only Admin and DealerManager can update configuration.");
                }

                if (dto.AllowOverlappingAppointments.HasValue)
                    config.AllowOverlappingAppointments = dto.AllowOverlappingAppointments.Value;

                if (dto.MaxConcurrentAppointments.HasValue)
                {
                    if (dto.MaxConcurrentAppointments.Value <= 0)
                        return Error(400, "Maximum concurrent appointments must be greater than 0.");
                    config.MaxConcurrentAppointments = dto.MaxConcurrentAppointments.Value;
                }

                if (dto.OpenTime.HasValue)
                    config.OpenTime = dto.OpenTime.Value;

                if (dto.CloseTime.HasValue)
                    config.CloseTime = dto.CloseTime.Value;

                if (dto.MinIntervalBetweenAppointments.HasValue)
                {
                    if (dto.MinIntervalBetweenAppointments.Value <= 0)
                        return Error(400, "Minimum interval must be greater than 0 minutes.");
                    config.MinIntervalBetweenAppointments = dto.MinIntervalBetweenAppointments.Value;
                }

                if (dto.BreakTimeBetweenAppointments.HasValue)
                {
                    if (dto.BreakTimeBetweenAppointments.Value < 0)
                        return Error(400, "Break time cannot be negative.");
                    config.BreakTimeBetweenAppointments = dto.BreakTimeBetweenAppointments.Value;
                }

                if (dto.MinDepositPercentage.HasValue)
                    config.MinDepositPercentage = dto.MinDepositPercentage.Value;

                if (dto.MaxDepositPercentage.HasValue)
                    config.MaxDepositPercentage = dto.MaxDepositPercentage.Value;

                _unitOfWork.DealerConfigurationRepository.Update(config);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Dealer configuration created/updated successfully."
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"An error occurred when upserting dealer configuration: {ex.Message}"
                };
            }
        }

        #endregion

        #region Update all deposit settings (giống DepositSettingService.UpdateAllSettings)

        public async Task<ResponseDTO> UpdateAllDepositSettingsAsync(ClaimsPrincipal userClaim, UpdateAllDepositSettingsDTO settingsDTO, CancellationToken ct)
        {
            try
            {
                var userId = GetUserId(userClaim);
                if (userId is null)
                    return Error(401, "User not login yet.");

                var role = GetUserRole(userClaim);
                if (!role?.Equals(StaticUserRole.Admin) ?? true)
                    return Error(403, "Only Admin can update all deposit settings.");

                var adminConfig = await _unitOfWork.DealerConfigurationRepository.GetByUserIdAsync(userId, ct);
                if (adminConfig is null)
                    return Error(404, "Admin dealer configuration not found.");

                if (!IsValidDepositRange(settingsDTO.MinDepositPercentage, settingsDTO.MaxDepositPercentage, out var error))
                    return Error(400, error!);

                if (settingsDTO.MaxDepositPercentage is not null)
                    adminConfig.MaxDepositPercentage = settingsDTO.MaxDepositPercentage.Value;

                if (settingsDTO.MinDepositPercentage is not null)
                    adminConfig.MinDepositPercentage = settingsDTO.MinDepositPercentage.Value;

                _unitOfWork.DealerConfigurationRepository.Update(adminConfig);

                var allConfigs = await _unitOfWork.DealerConfigurationRepository.GetAllAsync();
                var configsToUpdate = allConfigs
                    .Where(dc => dc.DealerId is not null
                                 && dc.ManagerId != userId
                                 && dc.MaxDepositPercentage > adminConfig.MaxDepositPercentage);

                foreach (var cfg in configsToUpdate)
                {
                    cfg.MaxDepositPercentage = adminConfig.MaxDepositPercentage;
                    _unitOfWork.DealerConfigurationRepository.Update(cfg);
                }

                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "All deposit settings updated successfully."
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"An error occurred that update all deposit settings at DealerConfigurationService: {ex.Message}"
                };
            }
        }

        #endregion

        #region Generate time slots (gom từ AppointmentSettingService.GenerateTimeSlotAsync)

        public async Task<ResponseDTO> GenerateTimeSlotAsync(ClaimsPrincipal userClaim, DateTime? targetDate = null, CancellationToken ct = default)
        {
            try
            {
                var userId = GetUserId(userClaim);
                if (userId is null)
                    return Error(404, "User not found");

                var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerOrStaffAsync(userId, ct);
                if (dealer == null)
                    return Error(404, "Dealer not found");

                var config = await _unitOfWork.DealerConfigurationRepository.GetByDealerIdAsync(dealer.Id, ct)
                             ?? await _unitOfWork.DealerConfigurationRepository.GetByDefaultAsync(ct);

                if (config == null)
                    return Error(404, "Dealer configuration (appointment setting) not found");

                // Lấy lịch hẹn trong ngày
                var date = (targetDate ?? DateTime.UtcNow).Date;
                var appointments = await _unitOfWork.AppointmentRepository
                    .GetByDealerIdAndDateAsync(dealer.Id, date);

                var slots = new List<GetAppointmentSlotDTO>();
                var currentTime = config.OpenTime;
                var interval = TimeSpan.FromMinutes(config.MinIntervalBetweenAppointments);
                var breakTime = TimeSpan.FromMinutes(config.BreakTimeBetweenAppointments);
                var end = config.CloseTime;

                while (currentTime + interval <= end)
                {
                    var slotStartTime = currentTime;
                    var slotEndTime = currentTime + interval;

                    var overlappingAppointments = appointments
                        .Where(a =>
                            (a.StartTime.TimeOfDay < slotEndTime) &&
                            (a.EndTime.TimeOfDay > currentTime) &&
                            a.Status == AppointmentStatus.Active)
                        .ToList();

                    bool isAvailable;
                    if (config.AllowOverlappingAppointments)
                    {
                        isAvailable = overlappingAppointments.Count < config.MaxConcurrentAppointments;
                    }
                    else
                    {
                        isAvailable = overlappingAppointments.Count == 0;
                    }

                    slots.Add(new GetAppointmentSlotDTO
                    {
                        OpenTime = slotStartTime,
                        CloseTime = slotEndTime,
                        IsAvailable = isAvailable
                    });

                    currentTime += interval;

                    if (currentTime + interval <= end)
                        currentTime += breakTime;
                }

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Appointment slots generated successfully",
                    Result = slots
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = ex.Message
                };
            }
        }

        #endregion

        #region Get default configuration
        public async Task<ResponseDTO> GetDefaultConfigurationAsync(CancellationToken ct)
        {
            try
            {
                var defaultConfig = await _unitOfWork.DealerConfigurationRepository.GetByDefaultAsync(ct);

                var getdefaultConfigDTO = _mapper.Map<GetDealerConfigurationDTO>(defaultConfig);
                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Default dealer configuration retrieved successfully.",
                    Result = getdefaultConfigDTO
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"An error occurred when getting default dealer configuration: {ex.Message}"
                };
            }
        }
        #endregion
    }
}
