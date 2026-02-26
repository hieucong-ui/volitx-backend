using AutoMapper;
using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.EVCInventory;
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
    public class EVCInventoryService : IEVCInventoryService
    {
        public readonly IUnitOfWork _unitOfWork;
        public readonly IMapper _mapper;
        public EVCInventoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ResponseDTO> CreateEVCInventoryAsync(CreateEVCInventoryDTO createEVCInventoryDTO)
        {
            try
            {
                var isEVCInventoryExists = await _unitOfWork.EVCInventoryRepository.IsEVCInventoryExistsByName(createEVCInventoryDTO.Name);
                if (isEVCInventoryExists)
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = false,
                        Message = "EVC Inventory already exists.",
                        StatusCode = 400
                    };
                }

                EVCInventory evcInventory = new EVCInventory
                {
                   Name = createEVCInventoryDTO.Name,
                   Location = createEVCInventoryDTO.Location,
                   Description = createEVCInventoryDTO.Description,
                   IsActive = true,
                   CreatedAt = DateTime.UtcNow,
                };

                await _unitOfWork.EVCInventoryRepository.AddAsync(evcInventory, CancellationToken.None);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO()
                {
                    IsSuccess = true,
                    Message = "EVC Inventory created successfully.",
                    StatusCode = 201,
                    Result = evcInventory
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

        public async Task<ResponseDTO> DeleteEVCInventoryAsync(Guid evcInventoryId)
        {
            try
            {
                var evcInventory = await _unitOfWork.EVCInventoryRepository.GetByIdAsync(evcInventoryId);
                if(evcInventory is null)
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = false,
                        Message = "EVC Inventory not found.",
                        StatusCode = 404
                    };
                }

                evcInventory.IsActive = false; // Soft delete by setting IsActive to false
                _unitOfWork.EVCInventoryRepository.Update(evcInventory);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO()
                {
                    IsSuccess = true,
                    Message = "EVC Inventory deleted successfully.",
                    StatusCode = 200
                };
            }
            catch
            {
                return new ResponseDTO()
                {
                    IsSuccess = false,
                    Message = "Internal server error.",
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> GetAllEVCInventoriesAsync()
        {
            try
            {
                var evcInventories = (await _unitOfWork.EVCInventoryRepository.GetAllAsync())
                    .Where(e => e.IsActive) // Only get active inventories
                    .ToList();
                if(evcInventories is null || evcInventories.Count == 0)
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = false,
                        Message = "No EVC Inventories found.",
                        StatusCode = 404
                    };
                }

                var getEVCInventories = _mapper.Map<List<GetEVCInventoryDTO>>(evcInventories);
                return new ResponseDTO()
                {
                    IsSuccess = true,
                    Message = "Get all EVC Inventories successfully.",
                    StatusCode = 200,
                    Result = getEVCInventories
                };
            }
            catch
            {
                return new ResponseDTO()
                {
                    IsSuccess = false,
                    Message = "Internal server error.",
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> GetEVCInventoryByIdAsync(Guid evcInventoryId)
        {
            try
            {
                var evcInventory = await _unitOfWork.EVCInventoryRepository.GetByIdAsync(evcInventoryId);
                if(evcInventory is null)
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = false,
                        Message = "EVC Inventory not found.",
                        StatusCode = 404
                    };
                }
                var getEVCInventory = _mapper.Map<GetEVCInventoryDTO>(evcInventory);
                return new ResponseDTO()
                {
                    IsSuccess = true,
                    Message = "Get EVC Inventory successfully.",
                    StatusCode = 200,
                    Result = getEVCInventory
                };
            }
            catch
            {
                return new ResponseDTO()
                {
                    IsSuccess = false,
                    Message = "Internal server error.",
                    StatusCode = 500
                };
            }
        }

        public Task<ResponseDTO> UpdateEVCInventoryAsync(Guid evcInventoryId, UpdateEVCInventory updateEVCInventory)
        {
            throw new NotImplementedException();
        }
        
    }
}
