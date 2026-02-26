using Voltix.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Infrastructure.IRepository
{
    public interface IDealerDebtRepository : IRepository<DealerDebt>
    {
        Task<DealerDebt?> GetPrevPeriodAsync(Guid dealerId, DateTime currentPeriodFromUtc, CancellationToken ct);
        Task<DealerDebt?> GetByDealerAndPeriodTrackedAsync(Guid dealerId, DateTime periodFrom, DateTime periodTo, CancellationToken ct);
        Task<DealerDebt> GetOrCreateQuarterAsync(Guid dealerId, DateTime occurredAtUtc, CancellationToken ct);
        (DateTime from, DateTime to) GetQuarterRangeUtc(DateTime asOfUtc);
    }
}
