using AutoMapper;
using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.ElectricVehicleModel;
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
    public class ElectricVehicleModelService : IElectricVehicleModelService
    {
        public readonly IUnitOfWork _unitOfWork;
        public readonly IMapper _mapper;

        public ElectricVehicleModelService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<ResponseDTO> CreateModelAsync(CreateElectricVehicleModelDTO createElectricVehicleModelDTO)
        {
            try
            {
                var isModelNameExist = await _unitOfWork.ElectricVehicleModelRepository
                    .IsModelExistsByName(createElectricVehicleModelDTO.ModelName);
                if (isModelNameExist)
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = false,
                        Message = "Model name already exists.",
                        StatusCode = 404
                    };
                }
                ElectricVehicleModel electricVehicleModel = new ElectricVehicleModel
                {
                    ModelName = createElectricVehicleModelDTO.ModelName,
                    Description = createElectricVehicleModelDTO.Description,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    Status = createElectricVehicleModelDTO.Status
                };

                if (electricVehicleModel is null)
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = false,
                        Message = "Model is null.",
                        StatusCode = 400
                    };
                }

                await _unitOfWork.ElectricVehicleModelRepository.AddAsync(electricVehicleModel, CancellationToken.None);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO()
                {
                    IsSuccess = true,
                    Message = "Create model successfully.",
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

        public async Task<ResponseDTO> DeleteModelAsync(Guid modelId)
        {
            try
            {
                var model = await _unitOfWork.ElectricVehicleModelRepository.GetByIdsAsync(modelId);
                if (model is null)
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = false,
                        Message = "Model not found.",
                        StatusCode = 404
                    };
                }
                model.IsActive = false; // Soft delete by setting IsActive to false
                _unitOfWork.ElectricVehicleModelRepository.Update(model);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO()
                {
                    IsSuccess = true,
                    Message = "Delete model successfully.",
                    StatusCode = 200
                };
            }
            catch
            {
                return new ResponseDTO()
                {
                    IsSuccess = false,
                    Message = "Delete model failed.",
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> GetAllModelsAsync()
        {
            try
            {
                var models = (await _unitOfWork.ElectricVehicleModelRepository.GetAllAsync()).Where(m => m.IsActive == true);
                var getModels = _mapper.Map<List<GetElectricVehicleModelDTO>>(models);
                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Model retrieve successfully",
                    Result = getModels,
                    StatusCode = 200,
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500,
                };

            }
        }

        public async Task<ResponseDTO> GetAllWithVersionAsync()
        {
            try
            {
                var models = await _unitOfWork.ElectricVehicleModelRepository.GetAllWithVersionAsync();
                var getModels = _mapper.Map<List<GetElectricVehicleModelDTO>>(models);
                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Model has at least 1 version successfully",
                    Result = getModels,
                    StatusCode = 200,
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500,
                };

            }
        }

        public async Task<ResponseDTO> GetModelByIdAsync(Guid modelId)
        {
            try
            {
                var model = await _unitOfWork.ElectricVehicleModelRepository.GetByIdsAsync(modelId);
                if (model is null)
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = false,
                        Message = "Model not found.",
                        StatusCode = 404
                    };
                }

                var getModel = _mapper.Map<GetElectricVehicleModelDTO>(model);

                return new ResponseDTO()
                {
                    IsSuccess = true,
                    Message = "Model retrieved successfully.",
                    Result = getModel,
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

        public async Task<ResponseDTO> GetModelByNameAsync(string modelName)
        {
            try
            {
                var model = await _unitOfWork.ElectricVehicleModelRepository.GetByNameAsync(modelName);
                if (model is null)
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = false,
                        Message = "Model not found.",
                        StatusCode = 404
                    };
                }

                var getModel = _mapper.Map<GetElectricVehicleModelDTO>(model);

                return new ResponseDTO()
                {
                    IsSuccess = true,
                    Message = "Model retrieved successfully.",
                    Result = getModel,
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

        public async Task<ResponseDTO> UpdateModelAsync(Guid modelId, UpdateElectricVehicleModelDTO updateElectricVehicleModelDTO)
        {
            try
            {
                var model = await _unitOfWork.ElectricVehicleModelRepository.GetByIdsAsync(modelId);
                if (model is null)
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = false,
                        Message = "Model not found.",
                        StatusCode = 404
                    };
                }

                if (!string.IsNullOrWhiteSpace(updateElectricVehicleModelDTO.ModelName))
                    model.ModelName = updateElectricVehicleModelDTO.ModelName;

                if (!string.IsNullOrWhiteSpace(updateElectricVehicleModelDTO.Description))
                    model.Description = updateElectricVehicleModelDTO.Description;


                _unitOfWork.ElectricVehicleModelRepository.Update(model);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO()
                {
                    IsSuccess = true,
                    Message = "Update model successfully.",
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
