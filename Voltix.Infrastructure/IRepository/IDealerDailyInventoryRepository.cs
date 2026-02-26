using Voltix.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Infrastructure.IRepository
{
    public interface IDealerDailyInventoryRepository
    {
        Task<Dictionary<(Guid DealerId, Guid EVTemplateId), int>> GetClosingStockMapAsync(DateTime snapshotDateUtc, CancellationToken ct);
        Task<Dictionary<(Guid DealerId, Guid EVTemplateId), int>> GetOpeningStockAsync(DateTime snapshotDateUtc, CancellationToken ct);

        Task<DealerDailyInventory?> GetAsync(Guid dealerId, Guid evTemplateId, DateTime snapshotDateUtc, CancellationToken ct);
        Task UpsertRangeAsync(IEnumerable<DealerDailyInventory> rows, CancellationToken ct);
        Task<IEnumerable<DealerDailyInventory>?> GetRangeAsync(Guid dealerId, Guid evTemplateId, DateTime fromDate, DateTime toDate, CancellationToken ct);
        Task<DateTime?> GetMaxSnapshotDateAsync(CancellationToken ct);
        Task<IReadOnlyList<DealerDailyInventory>> GetByDateAsync(DateTime date, CancellationToken ct);
    }
}
