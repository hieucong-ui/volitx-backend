using Microsoft.EntityFrameworkCore;
using Voltix.Domain.Entities;
using Voltix.Infrastructure.Context;
using Voltix.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Infrastructure.Repository
{
    public class EVCInventoryRepository : Repository<EVCInventory>, IEVCInventoryRepository
    {
        public readonly ApplicationDbContext _context;
        public EVCInventoryRepository(ApplicationDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<EVCInventory?> GetByIdAsync(Guid evcInventoryId)
        {
            return await _context.EVCInventories
                .FirstOrDefaultAsync(evc => evc.Id == evcInventoryId);
        }

        public async Task<int> GetTotalEVCInventoryAsync(CancellationToken ct)
        {
            return await _context.EVCInventories.CountAsync(ct);
        }

        public async Task<bool> IsEVCInventoryExistsById(Guid evcInventoryId)
        {
            return await _context.EVCInventories
                .AnyAsync(evc => evc.Id == evcInventoryId);
        }

        public async Task<bool> IsEVCInventoryExistsByName(string name)
        {
           return await _context.EVCInventories
                .AnyAsync(evc => evc.Name == name);
        }
    }
}
