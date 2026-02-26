using AutoMapper;
using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.ElectricVehicleVersion;
using Voltix.Application.IServices;
using Voltix.Domain.Entities;
using Voltix.Domain.Enums;
using Voltix.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.Services
{
    public class ElectricVehicleVersionService : IElectricVehicleVersionService
    {
        public readonly IUnitOfWork _unitOfWork;
        public readonly IMapper _mapper;
        public ElectricVehicleVersionService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<ResponseDTO> CreateVersionAsync(CreateElectricVehicleVersionDTO createElectricVehicleVersionDTO)

        {
            try
            {
                var isVersionNameExist = await _unitOfWork.ElectricVehicleVersionRepository
                    .IsVersionExistsByName(createElectricVehicleVersionDTO.VersionName);
                if (isVersionNameExist)
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = false,
                        Message = "Version name already exists.",
                        StatusCode = 404
                    };
                }

                ElectricVehicleVersion electricVehicleVersion = new ElectricVehicleVersion
                {
                    ModelId = createElectricVehicleVersionDTO.ModelId,
                    VersionName = createElectricVehicleVersionDTO.VersionName,
                    MotorPower = createElectricVehicleVersionDTO.MotorPower,
                    BatteryCapacity = createElectricVehicleVersionDTO.BatteryCapacity,
                    RangePerCharge = createElectricVehicleVersionDTO.RangePerCharge,
                    TopSpeed = createElectricVehicleVersionDTO.TopSpeed,
                    Weight = createElectricVehicleVersionDTO.Weight,
                    Height = createElectricVehicleVersionDTO.Height,
                    ProductionYear = createElectricVehicleVersionDTO.ProductionYear,
                    Description = createElectricVehicleVersionDTO.Description,
                    IsActive = true,
                };

                if (electricVehicleVersion is null)
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = false,
                        Message = "Version is null.",
                        StatusCode = 400
                    };
                }

                await _unitOfWork.ElectricVehicleVersionRepository.AddAsync(electricVehicleVersion, CancellationToken.None);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO()
                {
                    IsSuccess = true,
                    Message = "Create version successfully.",
                    StatusCode = 201
                };

            }
            catch (Exception ex)
            {
                return new ResponseDTO()
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> DeleteVersionAsync(Guid versionId)
        {
            try
            {
                var version = await _unitOfWork.ElectricVehicleVersionRepository.GetByIdsAsync(versionId);
                if (version == null)
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = false,
                        Message = "Version not found.",
                        StatusCode = 404
                    };
                }

                version.IsActive = false;//soft delete by setting IsActive to false
                _unitOfWork.ElectricVehicleVersionRepository.Update(version);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO()
                {
                    IsSuccess = true,
                    Message = "Delete version successfully.",
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO()
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> GetAllAvailableVersionsByModelIdAsync(Guid modelId)
        {
            try
            {
                var versions = await _unitOfWork.ElectricVehicleVersionRepository.GetAllVersionsByModelIdAsync(modelId);

                if (!versions.Any())
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "No versions found for the specified model.",
                        StatusCode = 404
                    };
                }

                var getVersions = _mapper.Map<List<GetElectricVehicleVersionDTO>>(versions);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Get all versions by model successfully.",
                    StatusCode = 200,
                    Result = getVersions
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

        public async Task<ResponseDTO> GetAllAvailableVersionsForBookingByModelIdAsync(Guid modelId)
        {
            try
            {
                var vehicles = await _unitOfWork.ElectricVehicleRepository.GetAvailableVehicleForBookingByModelIdAsync(modelId);
                if (!vehicles.Any())
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = false,
                        Message = "No available versions found for the specified model.",
                        StatusCode = 404
                    };
                }

                var availableVersions = vehicles
                    .Select(ev => ev.ElectricVehicleTemplate.Version)
                    .DistinctBy(v => v.Id)
                    .Select(v => _mapper.Map<GetElectricVehicleVersionDTO>(v))
                    .ToList();


                if (!availableVersions.Any())
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = false,
                        Message = "No available versions found for the specified model.",
                        StatusCode = 404
                    };
                }

                return new ResponseDTO()
                {
                    IsSuccess = true,
                    Message = "Get all available versions by model successfully.",
                    StatusCode = 200,
                    Result = availableVersions
                };


            }
            catch(Exception ex)
            {
                return new ResponseDTO()
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> GetAllVersionsAsync()
        {
            try
            {
                var versions = (await _unitOfWork.ElectricVehicleVersionRepository.GetAllAsync()).Where(v => v.IsActive == true);
                var getVersions = _mapper.Map<List<GetElectricVehicleVersionDTO>>(versions);
                return new ResponseDTO()
                {
                    IsSuccess = true,
                    Message = "Get all versions by model successfully.",
                    StatusCode = 200,
                    Result = getVersions
                };
            }catch(Exception ex)
            {
                return new ResponseDTO()
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> GetVersionByIdAsync(Guid versionId)
        {
            try
            {
                var version = await _unitOfWork.ElectricVehicleVersionRepository.GetByIdsAsync(versionId);
                if (version == null)
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = false,
                        Message = "Version not found.",
                        StatusCode = 404
                    };
                }

                var getVersion = _mapper.Map<GetElectricVehicleVersionDTO>(version);

                return new ResponseDTO()
                {
                    IsSuccess = true,
                    Message = "Get version successfully.",
                    StatusCode = 200,
                    Result = getVersion
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO()
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> GetVersionByNameAsync(string versionName)
        {
            try
            {
                var version = await _unitOfWork.ElectricVehicleVersionRepository.GetByNameAsync(versionName);
                if (version == null)
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = false,
                        Message = "Version not found.",
                        StatusCode = 404
                    };
                }

                var getVersion = _mapper.Map<GetElectricVehicleVersionDTO>(version);
                return new ResponseDTO()
                {
                    IsSuccess = true,
                    Message = "Get version successfully.",
                    StatusCode = 200,
                    Result = getVersion
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO()
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> UpdateVersionAsync(Guid versionId, UpdateElectricVehicleVersionDTO updateElectricVehicleVersionDTO)
        {
            try
            {
                var version = await _unitOfWork.ElectricVehicleVersionRepository.GetByIdsAsync(versionId);
                if (version == null)
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = false,
                        Message = "Version not found.",
                        StatusCode = 404
                    };
                }

                if (!string.IsNullOrWhiteSpace(updateElectricVehicleVersionDTO.VersionName))
                    version.VersionName = updateElectricVehicleVersionDTO.VersionName;
                if (updateElectricVehicleVersionDTO.MotorPower.HasValue && updateElectricVehicleVersionDTO.MotorPower.Value > 0)
                    version.MotorPower = updateElectricVehicleVersionDTO.MotorPower.Value;

                if (updateElectricVehicleVersionDTO.BatteryCapacity.HasValue && updateElectricVehicleVersionDTO.BatteryCapacity.Value > 0)
                    version.BatteryCapacity = updateElectricVehicleVersionDTO.BatteryCapacity.Value;

                if (updateElectricVehicleVersionDTO.RangePerCharge.HasValue && updateElectricVehicleVersionDTO.RangePerCharge.Value > 0)
                    version.RangePerCharge = updateElectricVehicleVersionDTO.RangePerCharge.Value;

                if (updateElectricVehicleVersionDTO.TopSpeed.HasValue && updateElectricVehicleVersionDTO.TopSpeed.Value >= 0)
                    version.TopSpeed = updateElectricVehicleVersionDTO.TopSpeed.Value;

                if (updateElectricVehicleVersionDTO.Weight.HasValue && updateElectricVehicleVersionDTO.Weight.Value >= 0)
                    version.Weight = updateElectricVehicleVersionDTO.Weight.Value;

                if (updateElectricVehicleVersionDTO.Height.HasValue && updateElectricVehicleVersionDTO.Height.Value >= 0)
                    version.Height = updateElectricVehicleVersionDTO.Height.Value;

                if (!string.IsNullOrWhiteSpace(updateElectricVehicleVersionDTO.Description))
                    version.Description = updateElectricVehicleVersionDTO.Description;

                _unitOfWork.ElectricVehicleVersionRepository.Update(version);
                await _unitOfWork.SaveAsync();

                if (version is null)
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = false,
                        Message = "Version is null.",
                        StatusCode = 400
                    };
                }

                return new ResponseDTO()
                {
                    IsSuccess = true,
                    Message = "Update version successfully.",
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO()
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500
                };
            }
        }

    }
}
