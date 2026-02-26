using AutoMapper;
using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.Warehouse;
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
    public class WarehouseService : IWarehouseService
    {
        public readonly IUnitOfWork _unitOfWork;
        public readonly IMapper _mapper;
        public WarehouseService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<ResponseDTO> CreateWarehouseAsync(CreateWarehouseDTO createWarehouseDTO)
        {
            try
            {
                
                Warehouse warehouse = new Warehouse
                {
                    DealerId = createWarehouseDTO.DealerId,
                    EVCInventoryId = createWarehouseDTO.EVCInventoryId,
                    WarehouseType = createWarehouseDTO.WarehouseType,
                    WarehouseName = createWarehouseDTO.WarehouseName,
                };

                await _unitOfWork.WarehouseRepository.AddAsync(warehouse, CancellationToken.None);
                await _unitOfWork.SaveAsync();
                var getWarehouse = _mapper.Map<GetWarehouseDTO>(warehouse);

                return new ResponseDTO
                {
                    StatusCode = 201,
                    Message = "Warehouse created successfully.",
                    Result = getWarehouse
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    StatusCode = 500,
                    Message = "An error occurred while creating the warehouse.",
                    Result = ex.Message
                };
            }
        }

        public async Task<ResponseDTO> GetAllEVCWarehouse()
        {
            try
            {
                var warehouses = await _unitOfWork.WarehouseRepository.GetAllAsync(w => w.WarehouseType == WarehouseType.EVInventory);
                if (warehouses == null)
                {
                    return new ResponseDTO
                    {
                        StatusCode = 404,
                        Message = "No EVC Warehouses found.",
                        Result = null
                    };
                }

                var getAllEVCWarehouses = _mapper.Map<List<GetWarehouseDTO>>(warehouses);
                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "EVC Warehouses retrieved successfully.",
                    Result = getAllEVCWarehouses
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    StatusCode = 500,
                    Message = ex.Message,
                    IsSuccess = false
                };
            }
        }

        public async Task<ResponseDTO> GetAllWarehousesAsync()
        {
            try
            {
                var warehouses = await _unitOfWork.WarehouseRepository.GetAllAsync();
                var getAllWarehouses = _mapper.Map<List<GetWarehouseDTO>>(warehouses);
                return new ResponseDTO
                {
                    StatusCode = 200,
                    Message = "Warehouses retrieved successfully.",
                    Result = getAllWarehouses
                };

            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    StatusCode = 500,
                    Message = "An error occurred while retrieving warehouses.",
                    Result = ex.Message
                };
            }
        }

        public async Task<ResponseDTO> GetWarehouseByIdAsync(Guid warehouseId)
        {
            try
            {
                var warehouse = await _unitOfWork.WarehouseRepository.GetWarehouseByIdAsync(warehouseId);
                if (warehouse == null)
                {
                    return new ResponseDTO
                    {
                        StatusCode = 404,
                        Message = "Warehouse not found.",
                        Result = null
                    };
                }
                var getWarehouse = _mapper.Map<GetWarehouseDTO>(warehouse);

                return new ResponseDTO
                {
                    StatusCode = 200,
                    Message = "Warehouse retrieved successfully.",
                    Result = getWarehouse
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    StatusCode = 500,
                    Message = "An error occurred while retrieving the warehouse.",
                    Result = ex.Message
                };
            }
        }
    }
}
