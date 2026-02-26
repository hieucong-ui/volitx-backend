using Voltix.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Infrastructure.IRepository
{
    public interface IDealerMemberRepository : IRepository<DealerMember>
    {
        Task<bool> IsExistDealerMemberByEmailAsync(Guid dealerId, string email, CancellationToken ct);
        Task<bool> IsActiveDealerMemberByEmailAsync(Guid dealerId, string email, CancellationToken ct);
        Task<int> TotalDealerMember(Guid dealerId, CancellationToken ct);
        Task<bool> IsDealerMemberBelongDealer(Guid dealerId, string applicationUserId, CancellationToken ct);
        Task<DealerMember?> GetByApplicationId(string applicationUserId, CancellationToken ct);
    }
}
