using AutoMapper;
using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.ElectricVehicleColor;
using Voltix.Application.IServices;
using Voltix.Domain.Entities;
using Voltix.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.Services
{
    public class ElectricVehicleColorService : IElectricVehicleColorService
    {
        public readonly IUnitOfWork _unitOfWork;
        public readonly IMapper _mapper;
        public ElectricVehicleColorService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<ResponseDTO> CreateColorAsync(CreateElectricVehicleColorDTO createElectricVehicleColorDTO)
        {
            try
            {
                var isColorCodeExist = await _unitOfWork.ElectricVehicleColorRepository
                    .IsColorExistsByCode(createElectricVehicleColorDTO.ColorCode);
                if (isColorCodeExist)
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = false,
                        Message = "Color code already exists.",
                        StatusCode = 404
                    };
                }
                var isColorNameExist = await _unitOfWork.ElectricVehicleColorRepository.IsColorExistsByName(createElectricVehicleColorDTO.ColorName);
                if (isColorNameExist)
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = false,
                        Message = "Color name already exists.",
                        StatusCode = 404
                    };
                }

                ElectricVehicleColor electricVehicleColor = new ElectricVehicleColor
                {
                    ColorCode = createElectricVehicleColorDTO.ColorCode,
                    ColorName = createElectricVehicleColorDTO.ColorName,
                    ExtraCost = createElectricVehicleColorDTO.ExtraCost
                };

                await _unitOfWork.ElectricVehicleColorRepository.AddAsync(electricVehicleColor, CancellationToken.None);
                await _unitOfWork.SaveAsync();
                return new ResponseDTO()
                {
                    IsSuccess = true,
                    Message = "Color created successfully.",
                    StatusCode = 201
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

        public Task<ResponseDTO> DeleteColorAsync(Guid colorId)
        {
            throw new NotImplementedException();
        }

        public async Task<ResponseDTO> GetAllColorsAsync()
        {
            try
            {
                var colors = await _unitOfWork.ElectricVehicleColorRepository.GetAllAsync();
                var getColors = _mapper.Map<List<GetElectricVehicleColorDTO>>(colors);
                return new ResponseDTO()
                {
                    IsSuccess = true,
                    Message = "Colors retrieved successfully.",
                    Result = getColors,
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

        public async Task<ResponseDTO> GetAllColorsByModelIdAndVersionIdAsync(Guid modelId, Guid versionId)
        {
            try
            {
                var colors = await _unitOfWork.ElectricVehicleColorRepository
                    .GetAllColorsByModelIdAndVersionIdAsync(modelId, versionId);

                if (!colors.Any())
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "No colors found for the specified model and version.",
                        StatusCode = 404
                    };
                }

                var colorDTOs = colors
                    .Select(c => _mapper.Map<GetElectricVehicleColorDTO>(c))
                    .ToList();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Get all colors by model and version successfully.",
                    StatusCode = 200,
                    Result = colorDTOs
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

        public async Task<ResponseDTO> GetAvailableColorsByModelIdAndVersionIdAsync(Guid modelId, Guid versionId)
        {
            try
            {
                var colors = await _unitOfWork.ElectricVehicleColorRepository.GetAvailableColorsByModelIdAndVersionIdAsync(modelId, versionId);
                if (colors is null)
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = false,
                        Message = "No colors found for the specified model and version.",
                        StatusCode = 404
                    };
                }

                var getColors = _mapper.Map<List<GetElectricVehicleColorDTO>>(colors);

                return new ResponseDTO()
                {
                    IsSuccess = true,
                    Message = "Colors retrieved successfully.",
                    Result = getColors,
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

        public async Task<ResponseDTO> GetColorByCodeAsync(string colorCode)
        {
            try
            {
                var color = await _unitOfWork.ElectricVehicleColorRepository.GetByCodeAsync(colorCode);
                if (color is null)
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = false,
                        Message = "Color not found.",
                        StatusCode = 404
                    };
                }
                var getColor = _mapper.Map<GetElectricVehicleColorDTO>(color);
                return new ResponseDTO()
                {
                    IsSuccess = true,
                    Message = "Color retrieved successfully.",
                    Result = getColor,
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

        public async Task<ResponseDTO> GetColorByIdAsync(Guid colorId)
        {
            try
            {
                var color = await _unitOfWork.ElectricVehicleColorRepository.GetByIdsAsync(colorId);
                if(color is null)
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = false,
                        Message = "Color not found.",
                        StatusCode = 404
                    };
                }

                var getColor = _mapper.Map<GetElectricVehicleColorDTO>(color);
                return new ResponseDTO()
                {
                    IsSuccess = true,
                    Message = "Color retrieved successfully.",
                    Result = getColor,
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

        public async Task<ResponseDTO> GetColorByNameAsync(string colorName)
        {
            try
            {
                var color = await _unitOfWork.ElectricVehicleColorRepository.GetByNameAsync(colorName);
                if (color is null)
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = false,
                        Message = "Color not found.",
                        StatusCode = 404
                    };
                }
                var getColor = _mapper.Map<GetElectricVehicleColorDTO>(color);
                return new ResponseDTO()
                {
                    IsSuccess = true,
                    Message = "Color retrieved successfully.",
                    Result = getColor,
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

        public async Task<ResponseDTO> UpdateColorAsync(Guid colorId, UpdateElectricVehicleColor updateElectricVehicleColor)
        {
            try
            {
                var color = await _unitOfWork.ElectricVehicleColorRepository.GetByIdsAsync(colorId);

                if (color is null)
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = false,
                        Message = "Color not found.",
                        StatusCode = 404
                    };
                }


                if (!string.IsNullOrWhiteSpace(updateElectricVehicleColor.ColorName))
                    color.ColorName = updateElectricVehicleColor.ColorName;

                if (!string.IsNullOrWhiteSpace(updateElectricVehicleColor.ColorCode))
                {
                    if (!updateElectricVehicleColor.ColorCode.StartsWith("#") || updateElectricVehicleColor.ColorCode.Length != 7)
                    {
                        return new ResponseDTO()
                        {
                            IsSuccess = false,
                            Message = "Color code must be in hex format (e.g., #FFFFFF).",
                            StatusCode = 400
                        };
                    }
                    color.ColorCode = updateElectricVehicleColor.ColorCode;
                }

                if (updateElectricVehicleColor.ExtraCost.HasValue && updateElectricVehicleColor.ExtraCost.Value >= 0)
                    color.ExtraCost = updateElectricVehicleColor.ExtraCost.Value;

                _unitOfWork.ElectricVehicleColorRepository.Update(color);
                await _unitOfWork.SaveAsync();
                return new ResponseDTO()
                {
                    IsSuccess = true,
                    Message = "Color updated successfully.",
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
