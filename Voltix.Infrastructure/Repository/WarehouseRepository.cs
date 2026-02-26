using Microsoft.EntityFrameworkCore;
using Voltix.Domain.Entities;
using Voltix.Domain.Enums;
using Voltix.Infrastructure.Context;
using Voltix.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Infrastructure.Repository
{
    public class WarehouseRepository : Repository<Warehouse>, IWarehouseRepository
    {
        public readonly ApplicationDbContext _context;
        public WarehouseRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<Warehouse?> GetWarehouseByDealerIdAsync(Guid dealerId)
        {
            return await _context.Warehouses.FirstOrDefaultAsync(w => w.DealerId == dealerId);
        }

        public async Task<Warehouse?> GetWarehouseByIdAsync(Guid warehouseId)
        {
            return await _context.Warehouses
                .Include(w => w.EVCInventory)
                .FirstOrDefaultAsync(w => w.Id == warehouseId);
                
        }

        public async Task<bool> IsWareHouseExistByDealerIdAsync(Guid dealerId)
        {
            return await _context.Warehouses
                .AnyAsync(w => w.DealerId == dealerId
                && w.WarehouseType == WarehouseType.Dealer);
        }

        public async Task<bool> IsWareHouseExistByEVCInventoryIdAsync(Guid evcInventoryId)
        {
            return await _context.Warehouses
                .AnyAsync(w => w.EVCInventoryId == evcInventoryId
                && w.WarehouseType == WarehouseType.EVInventory
                && w.EVCInventory.IsActive);
        }

        
    }
}
