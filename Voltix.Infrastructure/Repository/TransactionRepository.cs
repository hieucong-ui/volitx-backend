using Microsoft.EntityFrameworkCore;
using Voltix.Domain.Entities;
using Voltix.Domain.Enums;
using Voltix.Infrastructure.Context;
using Voltix.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Infrastructure.Repository
{
    public class TransactionRepository : Repository<Transaction>, ITransactionRepository
    {
        private readonly ApplicationDbContext _context;
        public TransactionRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<decimal> GetDealerRevenueAsync(Guid dealerId, CancellationToken ct)
        {
            return  await _context.Transactions
                .Include(t => t.CustomerOrder)
                    .ThenInclude(co => co.Quote)
                .Where(t => t.CustomerOrder.Quote.DealerId == dealerId
                        && t.Status == TransactionStatus.Success)
                .SumAsync(t => t.Amount, ct);
        }

        public async Task<List<Transaction>> GetDealerTransactionsByYearAsync(Guid dealerId, int year, CancellationToken ct)
        {
            return await _context.Transactions
                    .Include(t => t.CustomerOrder)
                        .ThenInclude(co => co.Quote)
                    .Include(t => t.CustomerOrder)
                        .ThenInclude(co => co.OrderDetails)
                    .Where(t => t.CustomerOrder.Quote.DealerId == dealerId
                                && t.Status == TransactionStatus.Success
                                && t.CreatedAt.Year == year)
                    .ToListAsync(ct);
        }
        public async Task<Transaction?> GetByCustomerOrderIdAsync(Guid customerOrderId, CancellationToken ct)
        {
            return await _context.Transactions
                .FirstOrDefaultAsync(t => t.CustomerOrderId == customerOrderId, ct);
        }

        public async Task<bool> IsExistTransactionAsync(string method, string orderRef, CancellationToken ct)
        {
            return await _context.Transactions
                .AsNoTracking()
                .AnyAsync(t => t.Provider == method && t.OrderRef == orderRef, ct);
        }
    }
}
