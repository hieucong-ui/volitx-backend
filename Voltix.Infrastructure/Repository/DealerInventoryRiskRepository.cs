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
    public class DealerInventoryRiskRepository : Repository<DealerInventoryRisk>, IDealerInventoryRiskRepository
    {
        private readonly ApplicationDbContext _context;
        public DealerInventoryRiskRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task UpsertRangeAsync(IEnumerable<DealerInventoryRisk> risks, CancellationToken ct)
        {
            foreach (var r in risks)
            {
                var existing = await _context.DealerInventoryRisks
                    .FirstOrDefaultAsync(x =>
                        x.DealerId == r.DealerId &&
                        x.EVTemplateId == r.EVTemplateId &&
                        x.TargetDate == r.TargetDate, ct);

                if (existing == null)
                {
                    await _context.DealerInventoryRisks.AddAsync(r, ct);
                }
                else
                {
                    existing.ExpectedClosing = r.ExpectedClosing;
                    existing.RiskLevel = r.RiskLevel;
                    existing.IsResolved = r.IsResolved;
                    existing.CreatedAt = r.CreatedAt;
                    _context.DealerInventoryRisks.Update(existing);
                }
            }
        }
    }
}
