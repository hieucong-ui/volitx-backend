using Microsoft.EntityFrameworkCore;
using Voltix.Domain.Entities;
using Voltix.Infrastructure.Context;
using Voltix.Infrastructure.IRepository;

namespace Voltix.Infrastructure.Repository
{
    public class DealerDebtRepository : Repository<DealerDebt>, IDealerDebtRepository
    {
        private readonly ApplicationDbContext _context;
        public DealerDebtRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public (DateTime from, DateTime to) GetQuarterRangeUtc(DateTime asOfUtc)
        {
            var dt = DateTime.SpecifyKind(asOfUtc, DateTimeKind.Utc);

            int firstMonthOfQuarter = ((dt.Month - 1) / 3) * 3 + 1;

            var start = new DateTime(dt.Year, firstMonthOfQuarter, 1, 0, 0, 0, DateTimeKind.Utc);

            if ((dt.Month - firstMonthOfQuarter) == 2)
            {
                start = start.AddMonths(3);
            }

            var from = start;
            var to = from.AddMonths(3).AddTicks(-1);
            return (from, to);
        }


        public async Task<DealerDebt?> GetPrevPeriodAsync(Guid dealerId, DateTime currentPeriodFromUtc, CancellationToken ct)
        {
            return await _context.DealerDebts
                .Where(d => d.DealerId == dealerId && d.PeriodTo < currentPeriodFromUtc)
                .OrderByDescending(d => d.PeriodTo)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<DealerDebt?> GetByDealerAndPeriodTrackedAsync(Guid dealerId, DateTime periodFrom, DateTime periodTo, CancellationToken ct)
        {
            return await _context.DealerDebts
                .FirstOrDefaultAsync(d =>
                    d.DealerId == dealerId &&
                    d.PeriodFrom == periodFrom &&
                    d.PeriodTo == periodTo, ct);
        }

        public async Task<DealerDebt> GetOrCreateQuarterAsync(Guid dealerId, DateTime occurredAtUtc, CancellationToken ct)
        {
            var (from, to) = GetQuarterRangeUtc(occurredAtUtc);

            var period = await GetByDealerAndPeriodTrackedAsync(dealerId, from, to, ct);
            if (period != null) return period;

            var prev = await GetPrevPeriodAsync(dealerId, from, ct);

            period = new DealerDebt
            {
                Id = Guid.NewGuid(),
                DealerId = dealerId,
                PeriodFrom = from,
                PeriodTo = to,
                OpeningBalance = prev?.ClosingBalance ?? 0m,
            };

            await _context.DealerDebts.AddAsync(period, ct);
            await _context.SaveChangesAsync(ct);
            return period;
        }
    }
}
