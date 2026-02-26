using Microsoft.EntityFrameworkCore;
using Voltix.Domain.Entities;
using Voltix.Infrastructure.Context;
using Voltix.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Infrastructure.Repository
{
    public class DealerMemberRepository : Repository<DealerMember>, IDealerMemberRepository
    {
        private readonly ApplicationDbContext _context;
        public DealerMemberRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public Task<bool> IsActiveDealerMemberByEmailAsync(Guid dealerId, string email, CancellationToken ct)
        {
            return _context.DealerMembers
                .AnyAsync(dl => dl.DealerId == dealerId && dl.ApplicationUser.Email == email && dl.IsActive == true, ct);
        }

        public async Task<bool> IsDealerMemberBelongDealer(Guid dealerId, string applicationUserId, CancellationToken ct)
        {
            return await _context.DealerMembers
                .AnyAsync(dl => dl.DealerId == dealerId && dl.ApplicationUserId == applicationUserId, ct);
        }

        public async Task<bool> IsExistDealerMemberByEmailAsync(Guid dealerId, string email, CancellationToken ct)
        {
            return await _context.DealerMembers
                .AnyAsync(dl => dl.DealerId == dealerId && dl.ApplicationUser.Email == email, ct);
        }

        public async Task<int> TotalDealerMember(Guid dealerId, CancellationToken ct)
        {
            return await _context.DealerMembers
                .Where(dm => dm.DealerId == dealerId && dm.IsActive == true)
                .CountAsync(ct);
        }

        public async Task<DealerMember?> GetByApplicationId(string applicationUserId, CancellationToken ct)
        {
            return await _context.DealerMembers
                .Include(dm => dm.ApplicationUser)
                .Where(dm => dm.ApplicationUserId == applicationUserId)
                .FirstOrDefaultAsync(ct);
        }
    }
}
