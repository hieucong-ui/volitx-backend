using Voltix.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Infrastructure.IRepository
{
    public interface IEContractRepository : IRepository<EContract>
    {
        Task<EContract?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<List<EContract>?> GetEContractDealerByDealerIdAsync(string managerId, CancellationToken ct);
        Task<EContract?> GetByBookingId(Guid bookingId, CancellationToken ct);

    }
}
