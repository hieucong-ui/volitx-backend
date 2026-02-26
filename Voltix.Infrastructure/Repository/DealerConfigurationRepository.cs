using Microsoft.EntityFrameworkCore;
using Voltix.Domain.Entities;
using Voltix.Infrastructure.Context;
using Voltix.Infrastructure.IRepository;

namespace Voltix.Infrastructure.Repository
{
    public class DealerConfigurationRepository : Repository<DealerConfiguration>, IDealerConfigurationRepository
    {
        private readonly ApplicationDbContext _context;

        public DealerConfigurationRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<DealerConfiguration?> GetByDealerIdAsync(Guid dealerId, CancellationToken ct)
        {
            return await _context.DealerConfigurations
                .Include(dc => dc.Dealer)
                .Include(dc => dc.Manager)
                .FirstOrDefaultAsync(dc => dc.DealerId == dealerId, ct);
        }

        public async Task<DealerConfiguration?> GetByDefaultAsync(CancellationToken ct)
        {
            return await _context.DealerConfigurations
                .Include(dc => dc.Dealer)
                .Include(dc => dc.Manager)
                .FirstOrDefaultAsync(dc => dc.DealerId == null, ct);
        }

        public async Task<DealerConfiguration?> GetByUserIdAsync(string userId, CancellationToken ct)
        {
            return await _context.DealerConfigurations
                .Include(dc => dc.Dealer)
                .Include(dc => dc.Manager)
                .FirstOrDefaultAsync(dc => dc.ManagerId == userId, ct);
        }
    }
}
