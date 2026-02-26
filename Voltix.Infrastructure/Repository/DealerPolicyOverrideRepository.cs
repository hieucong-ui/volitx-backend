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
    public class DealerPolicyOverrideRepository : Repository<DealerPolicyOverride>, IDealerPolicyOverrideRepository
    {
        private readonly ApplicationDbContext _context;
        public DealerPolicyOverrideRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<DealerPolicyOverride?> GetActiveByDealerAsync(Guid dealerId, CancellationToken ct)
        {
            return await _context.DealerPolicyOverrides
                .FirstOrDefaultAsync(dpo => dpo.DealerId == dealerId && dpo.IsActive);
        }

        public async Task<DealerPolicyOverride?> GetCurrentActiveAsync(Guid dealerId, DateTime now, CancellationToken ct)
        {
            return await _context.DealerPolicyOverrides
                .Where(x => x.DealerId == dealerId
                        && x.IsActive
                        && (x.EffectiveFrom == null || x.EffectiveFrom <= now)
                        && (x.EffectiveTo == null || x.EffectiveTo >= now))
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(ct);
        }
    }
}
