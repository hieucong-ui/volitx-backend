using Aspose.Words.Tables;
using AutoMapper;
using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.Promotion;
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
    public class PromotionService : IPromotionService
    {
        public readonly IUnitOfWork _unitOfWork;
        public readonly IMapper _mapper;
        public PromotionService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<ResponseDTO> CreatePromotionAsync(CreatePromotionDTO createPromotionDTO)
        {
            try
            {
                var isExistPromotion = await _unitOfWork.PromotionRepository.IsExistPromotionByNameAsync(createPromotionDTO.Name);
                if (isExistPromotion)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Promotion is exist"
                    };
                }
                if (createPromotionDTO.ModelId.HasValue)
                {
                    var existByModel = await _unitOfWork.PromotionRepository
                        .GetActivePromotionByModelIdAsync(createPromotionDTO.ModelId.Value);

                    if (existByModel != null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 400,
                            Message = "This model already has an active promotion."
                        };
                    }
                }

                if (createPromotionDTO.VersionId.HasValue)
                {
                    var existByVersion = await _unitOfWork.PromotionRepository
                        .GetActivePromotionByVersionIdAsync(createPromotionDTO.VersionId.Value);

                    if (existByVersion != null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 400,
                            Message = "This version already has an active promotion."
                        };
                    }
                }

                if (createPromotionDTO.StartDate >= createPromotionDTO.EndDate)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "StartDate must before EndDate",
                        StatusCode = 400
                    };
                }

                // Start Date and End Date can't in the past  
                if (createPromotionDTO.StartDate < DateTime.UtcNow || createPromotionDTO.EndDate < DateTime.UtcNow)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "StartDate and EndDate can't be in the past",
                        StatusCode = 400
                    };
                }

                if (createPromotionDTO.DiscountType == DiscountType.Percentage)
                {
                    if (createPromotionDTO == null || createPromotionDTO.Percentage < 0 || createPromotionDTO.Percentage > 100)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Percentage must in 0 to 100",
                            StatusCode = 400
                        };
                    }
                }
                else if (createPromotionDTO.DiscountType == DiscountType.FixAmount)
                {
                    if (createPromotionDTO.FixedAmount <= 0)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Fix Amount can't be lower than 0",
                            StatusCode = 400
                        };
                    }
                }

                Promotion promotion = new Promotion
                {
                    Name = createPromotionDTO.Name,
                    Description = createPromotionDTO.Description,
                    Percentage = createPromotionDTO?.Percentage,
                    FixedAmount = createPromotionDTO?.FixedAmount,
                    ModelId = createPromotionDTO?.ModelId,
                    VersionId = createPromotionDTO?.VersionId,
                    DiscountType = createPromotionDTO.DiscountType,
                    StartDate = createPromotionDTO.StartDate,
                    EndDate = createPromotionDTO.EndDate,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                if (promotion == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = "Invalid promotion"
                    };
                }

                await _unitOfWork.PromotionRepository.AddAsync(promotion, CancellationToken.None);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Create promotion successfully",
                    Result = promotion
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

        public async Task<ResponseDTO> DeletePromotionAsync(Guid promotionId)
        {
            try
            {
                var promotion = await _unitOfWork.PromotionRepository.GetPromotionByIdAsync(promotionId);
                if (promotion == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Promotion not exist"
                    };
                }

                promotion.IsActive = false;
                _unitOfWork.PromotionRepository.Update(promotion);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Delete promotion successfully"
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

        public async Task<ResponseDTO> GetAllAsync()
        {
            try
            {
                var allPromotions = await _unitOfWork.PromotionRepository.GetAllAsync();

                //Deactivate expired promotions
                foreach (var promo in allPromotions)
                {
                    if (promo.IsActive && promo.EndDate < DateTime.UtcNow)
                    {
                        promo.IsActive = false;
                        _unitOfWork.PromotionRepository.Update(promo);
                    }
                }
                await _unitOfWork.SaveAsync();

                var getPromotions = _mapper.Map<List<GetPromotionDTO>>(allPromotions);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Get active promotions successfully",
                    Result = getPromotions
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

        public async Task<ResponseDTO> GetPromotionByIdAsync(Guid promotionId)
        {
            try
            {
                var promotion = await _unitOfWork.PromotionRepository.GetPromotionByIdAsync(promotionId);
                if (promotion == null || !promotion.IsActive || promotion.StartDate > DateTime.UtcNow || promotion.EndDate < DateTime.UtcNow)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Promotion not exist or expired"
                    };
                }

                var getPromotion = _mapper.Map<GetPromotionDTO>(promotion);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Get promotion succesfully",
                    Result = getPromotion
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

        public async Task<ResponseDTO> GetPromotionByNameAsync(string name)
        {
            try
            {
                var promotions = await _unitOfWork.PromotionRepository.GetPromotionByNameAsync(name);
                if (promotions == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = " Promotion not found",
                        StatusCode = 404
                    };
                }

                var getPromotion = _mapper.Map<GetPromotionDTO>(promotions);
                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Get promotion name successfully",
                    StatusCode = 201,
                    Result = getPromotion
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

        public async Task<ResponseDTO> GetPromotionsForQuoteAsync(Guid? modelId, Guid? versionId)
        {
            try
            {
                var allPromotions = await _unitOfWork.PromotionRepository.GetAllAsync();

                //Deactive promotion expired
                foreach (var p in allPromotions)
                {
                    if (p.IsActive && p.EndDate < DateTime.UtcNow)
                    {
                        p.IsActive = false;
                        _unitOfWork.PromotionRepository.Update(p);
                    }
                }

                await _unitOfWork.SaveAsync();

                //Take valid promotion
                var validPromotions = allPromotions
                    .Where(p => p.IsActive && p.StartDate <= DateTime.UtcNow && p.EndDate >= DateTime.UtcNow).ToList();

                //Take promotion have model and version
                var specificPromotions = validPromotions
                    .Where(p => p.ModelId.HasValue
                           && p.VersionId.HasValue
                           && p.VersionId == versionId
                           && p.ModelId == modelId)
                    .ToList();

                // take all promotion
                var globalPromotion = validPromotions
                    .Where(p => !p.ModelId.HasValue && !p.VersionId.HasValue)
                    .ToList();

                // valid model and version
                if (modelId.HasValue && versionId.HasValue)
                {
                    var modelExists = await _unitOfWork.ElectricVehicleModelRepository.IsModelExistsById(modelId.Value);
                    var versionExists = await _unitOfWork.ElectricVehicleVersionRepository.IsVersionExistsById(versionId.Value);

                    if (!modelExists || !versionExists)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 404,
                            Message = "Selected Model or Version does not exist."
                        };
                    }
                }

                var totalPromotion = specificPromotions.Concat(globalPromotion).ToList();

                var getPromotions = _mapper.Map<List<GetPromotionDTO>>(totalPromotion);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Get promotion for quote successfully",
                    StatusCode = 200,
                    Result = getPromotions
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

        public async Task<ResponseDTO> UpdatePromotionAsync(Guid promotionId, UpdatePromotionDTO dto)
        {
            try
            {
                var promotion = await _unitOfWork.PromotionRepository.GetPromotionByIdAsync(promotionId);
                if (promotion == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Promotion not found"
                    };
                }

                // Validate Date
                if (dto.StartDate.HasValue && dto.EndDate.HasValue)
                {
                    if (dto.StartDate >= dto.EndDate)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 400,
                            Message = "Start Date must be before End Date"
                        };
                    }

                    if (dto.StartDate < DateTime.UtcNow || dto.EndDate < DateTime.UtcNow)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 400,
                            Message = "Start Date and End Date must be in the future"
                        };
                    }
                }

                // Validate Discount Type
                if (dto.DiscountType == DiscountType.Percentage)
                {
                    if (!dto.Percentage.HasValue || dto.Percentage < 0 || dto.Percentage > 100)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 400,
                            Message = "Percentage must be between 0 and 100"
                        };
                    }
                }
                else if (dto.DiscountType == DiscountType.FixAmount)
                {
                    if (!dto.FixedAmount.HasValue || dto.FixedAmount <= 0)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 400,
                            Message = "Fixed amount must be greater than 0"
                        };
                    }
                }


                // Validate name promotion if the same
                if (!string.IsNullOrWhiteSpace(dto.Name))
                {
                    var newName = dto.Name.Trim().ToUpper();
                    if (newName != promotion.Name)
                    {
                        var isExist = await _unitOfWork.PromotionRepository
                            .IsExistPromotionByNameExceptAsync(newName, promotion.Id);
                        if (isExist)
                        {
                            return new ResponseDTO
                            {
                                IsSuccess = false,
                                StatusCode = 400,
                                Message = "Promotion name already exists"
                            };
                        }

                        promotion.Name = newName;
                    }
                }

                // Orther
                if (!string.IsNullOrWhiteSpace(dto.Description))
                    promotion.Description = dto.Description.Trim();

                if (dto.StartDate.HasValue)
                    promotion.StartDate = dto.StartDate.Value;

                if (dto.EndDate.HasValue)
                    promotion.EndDate = dto.EndDate.Value;

                promotion.DiscountType = dto.DiscountType;

                if (dto.DiscountType == DiscountType.Percentage)
                {
                    promotion.Percentage = dto.Percentage;
                    promotion.FixedAmount = null;
                }
                else
                {
                    promotion.FixedAmount = dto.FixedAmount;
                    promotion.Percentage = null;
                }

                // Update ModelId and VersionId
                if (dto.ModelId.HasValue)
                    promotion.ModelId = dto.ModelId;
                if (dto.VersionId.HasValue)
                    promotion.VersionId = dto.VersionId;

                _unitOfWork.PromotionRepository.Update(promotion);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Promotion updated successfully",
                    Result = promotion
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
    }
}

