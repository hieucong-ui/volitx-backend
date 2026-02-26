using Voltix.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Infrastructure.IRepository
{
    public interface IEVCInventoryRepository :IRepository<EVCInventory>
    {
        Task<EVCInventory?> GetByIdAsync(Guid evcInventoryId);
        Task<bool> IsEVCInventoryExistsById(Guid evcInventoryId);
        Task<bool> IsEVCInventoryExistsByName(string name);
        Task<int> GetTotalEVCInventoryAsync(CancellationToken ct);
    }
}
