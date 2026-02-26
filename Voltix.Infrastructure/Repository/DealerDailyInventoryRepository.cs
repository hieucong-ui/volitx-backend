using Microsoft.EntityFrameworkCore;
using Voltix.Domain.Entities;
using Voltix.Infrastructure.Context;
using Voltix.Infrastructure.IRepository;

namespace Voltix.Infrastructure.Repository
{
    public class DealerDailyInventoryRepository : IDealerDailyInventoryRepository
    {
        private readonly ApplicationDbContext _context;
        public DealerDailyInventoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DealerDailyInventory?> GetAsync(Guid dealerId, Guid evTemplateId, DateTime snapshotDate, CancellationToken ct)
        {
            var dateOnly = snapshotDate.Date;
            return await _context.DealerDailyInventories
                .FirstOrDefaultAsync(x => x.DealerId == dealerId
                                       && x.EVTemplateId == evTemplateId
                                       && x.SnapshotDate == dateOnly, ct);
        }

        public async Task<IReadOnlyList<DealerDailyInventory>> GetByDateAsync(DateTime date, CancellationToken ct)
        {
            var dayOnly = date.Date;
            return await _context.DealerDailyInventories
                .AsNoTracking()
                .Where(x => x.SnapshotDate == dayOnly)
                .ToListAsync(ct);
        }

        public async Task<Dictionary<(Guid DealerId, Guid EVTemplateId), int>> GetClosingStockMapAsync(DateTime snapshotDateUtc, CancellationToken ct)
        {
            return await _context.DealerDailyInventories
                .AsNoTracking()
                .Where(x => x.SnapshotDate == snapshotDateUtc.Date)
                .ToDictionaryAsync(
                    keySelector: x => (x.DealerId, x.EVTemplateId),
                    elementSelector: x => x.ClosingStock,
                    cancellationToken: ct);
        }

        public async Task<DateTime?> GetMaxSnapshotDateAsync(CancellationToken ct)
        {
            return await _context.DealerDailyInventories
            .Select(x => (DateTime?)x.SnapshotDate)
            .MaxAsync(ct);
        }

        public async Task<Dictionary<(Guid DealerId, Guid EVTemplateId), int>> GetOpeningStockAsync(DateTime snapshotDateUtc, CancellationToken ct)
        {
            return await _context.DealerDailyInventories
                .AsNoTracking()
                .Where(x => x.SnapshotDate == snapshotDateUtc.Date)
                .ToDictionaryAsync(
                    keySelector: x => (x.DealerId, x.EVTemplateId),
                    elementSelector: x => x.OpeningStock,
                    cancellationToken: ct);
        }

        public async Task<IEnumerable<DealerDailyInventory>?> GetRangeAsync(Guid dealerId, Guid evTemplateId, DateTime fromDate, DateTime toDate, CancellationToken ct)
        {
            return await _context.DealerDailyInventories
                .AsNoTracking()
                .Where(x => x.DealerId == dealerId &&
                    x.EVTemplateId == evTemplateId &&
                    x.SnapshotDate >= fromDate &&
                    x.SnapshotDate <= toDate)
                .OrderBy(x => x.SnapshotDate)
                .ToListAsync(ct);
        }

        public async Task UpsertRangeAsync(IEnumerable<DealerDailyInventory> rows, CancellationToken ct)
        {
            foreach (var r in rows)
            {
                var existing = await _context.DealerDailyInventories
                    .FirstOrDefaultAsync(x => x.DealerId == r.DealerId
                                           && x.EVTemplateId == r.EVTemplateId
                                           && x.SnapshotDate == r.SnapshotDate.Date, ct);

                if (existing is null)
                {
                    await _context.DealerDailyInventories.AddAsync(r, ct);
                }
                else
                {
                    existing.OpeningStock = r.OpeningStock;
                    existing.Inflow = r.Inflow;
                    existing.Outflow = r.Outflow;
                    existing.ClosingStock = r.ClosingStock;
                    existing.Note = r.Note;
                    _context.DealerDailyInventories.Update(existing);
                }
            }
        }


    }
}
