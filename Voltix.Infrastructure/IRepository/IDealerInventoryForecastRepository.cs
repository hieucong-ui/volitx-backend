using Voltix.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Infrastructure.IRepository
{
    public interface IDealerInventoryForecastRepository : IRepository<DealerInventoryForecast>
    {
        Task UpsertRangeAsync(IEnumerable<DealerInventoryForecast> rows, CancellationToken ct);

        Task<IEnumerable<DealerInventoryForecast>> GetRangeAsync(Guid dealerId, Guid evTemplateId, DateTime fromDate, DateTime toDate, CancellationToken ct);
        Task<IReadOnlyList<DealerInventoryForecast>> GetForecastsInRangeAsync(DateTime from, DateTime to, CancellationToken ct);
    }
}
