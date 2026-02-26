using Microsoft.AspNetCore.Identity;
using Voltix.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Infrastructure.IRepository
{
    public interface IDealerRepository : IRepository<Dealer>
    {
        Task<Dealer?> GetByIdAsync(Guid dealerId, CancellationToken ct);
        Task<Dealer?> GetManagerByUserIdAsync(string userId, CancellationToken ct);
        Task<bool> IsExistByNameAsync(string name, CancellationToken ct);
        Task<bool> IsExistByIdAsync(Guid id, CancellationToken ct);
        Task<ApplicationUser?> GetManagerByDealerId(Guid dealerId, CancellationToken ct);
        Task<Dealer?> GetDealerByUserIdAsync(string userId, CancellationToken ct);
        Task<Dealer?> GetDealerByManagerIdAsync(string managerId, CancellationToken ct);
        Task<Dealer?> GetDealerByManagerOrStaffAsync(string userdId, CancellationToken ct);
        Task<Dealer?> GetTrackedDealerByManagerOrStaffAsync(string userId, CancellationToken ct);
        Task<int> GetTotalDealersAsync(CancellationToken ct);
    }
}
