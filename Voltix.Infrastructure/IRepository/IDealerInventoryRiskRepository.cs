using Voltix.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Infrastructure.IRepository
{
    public interface IDealerInventoryRiskRepository : IRepository<DealerInventoryRisk>
    {
        Task UpsertRangeAsync(IEnumerable<DealerInventoryRisk> risks, CancellationToken ct);
    }
}
