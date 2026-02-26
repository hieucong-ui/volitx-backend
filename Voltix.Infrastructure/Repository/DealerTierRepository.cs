using Microsoft.EntityFrameworkCore;
using Voltix.Domain.Entities;
using Voltix.Infrastructure.Context;
using Voltix.Infrastructure.IRepository;

namespace Voltix.Infrastructure.Repository
{
    public class DealerTierRepository : Repository<DealerTier>, IDealerTierRepository
    {
        private readonly ApplicationDbContext _context;
        public DealerTierRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<DealerTier?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            return await _context.DealerTiers.FindAsync(id , ct);
        }

        public async Task<DealerTier?> GetByLevelAsync(int level, CancellationToken ct)
        {
            return await _context.DealerTiers
                .FirstOrDefaultAsync(dt => dt.Level == level, ct);
        }
    }
}
