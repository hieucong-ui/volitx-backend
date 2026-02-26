using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.Warehouse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.IServices
{
    public interface IWarehouseService
    {
        Task<ResponseDTO> CreateWarehouseAsync(CreateWarehouseDTO createWarehouseDTO);
        Task<ResponseDTO> GetAllWarehousesAsync();
        Task<ResponseDTO> GetWarehouseByIdAsync(Guid warehouseId);
        Task<ResponseDTO> GetAllEVCWarehouse();

    }
}
