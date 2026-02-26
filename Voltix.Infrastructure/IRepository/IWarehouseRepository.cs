using Voltix.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Infrastructure.IRepository
{
    public interface IWarehouseRepository : IRepository<Warehouse>
    {
        Task<Warehouse?> GetWarehouseByIdAsync(Guid warehouseId);
        Task<bool> IsWareHouseExistByDealerIdAsync(Guid dealerId);
        Task<bool> IsWareHouseExistByEVCInventoryIdAsync(Guid evcInventoryId);
        Task<Warehouse?> GetWarehouseByDealerIdAsync(Guid dealerId);
    }
}
