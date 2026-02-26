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
    public class DealerDebtTransactionRepository : Repository<DealerDebtTransaction>, IDealerDebtTransactionRepository
    {
        private readonly ApplicationDbContext _context;
        public DealerDebtTransactionRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> IsDuplicated(Guid dealerId, string externalId, CancellationToken ct)
        {
            return await _context.DealerDebtTransactions
                .AsNoTracking()
                .AnyAsync(d => d.DealerId == dealerId && d.ExternalId == externalId, ct);
        }
    }
}
