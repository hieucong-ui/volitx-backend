using Voltix.Domain.Entities;

namespace Voltix.Infrastructure.IRepository
{
    public interface IDealerConfigurationRepository : IRepository<DealerConfiguration>
    {
        Task<DealerConfiguration?> GetByDealerIdAsync(Guid dealerId, CancellationToken ct);
        Task<DealerConfiguration?> GetByDefaultAsync(CancellationToken ct);
        Task<DealerConfiguration?> GetByUserIdAsync(string userId, CancellationToken ct);
    }
}
