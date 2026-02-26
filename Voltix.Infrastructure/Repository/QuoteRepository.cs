using Microsoft.EntityFrameworkCore;
using Voltix.Domain.Entities;
using Voltix.Infrastructure.Context;
using Voltix.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Infrastructure.Repository
{
    public class QuoteRepository : Repository<Quote> , IQuoteRepository
    {
        public readonly ApplicationDbContext _context;
        public QuoteRepository(ApplicationDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Task<int> CountByDealerIdAsync(Guid dealerId, CancellationToken ct)
        {
            return _context.Quotes.Where(q => q.DealerId == dealerId).CountAsync(ct);
        }

        public async Task<List<Quote>> GetAllQuotesWithDetailAsync(Expression<Func<Quote, bool>>? filter = null)
        {
            IQueryable<Quote> query = _context.Quotes
        .Include(q => q.QuoteDetails)
            .ThenInclude(qd => qd.ElectricVehicleVersion)
                .ThenInclude(v => v.Model)
        .Include(q => q.QuoteDetails)
            .ThenInclude(qd => qd.ElectricVehicleColor)
        .Include(q => q.QuoteDetails)
            .ThenInclude(qd => qd.Promotion)
        .Include(q => q.Dealer);

            if (filter != null)
                query = query.Where(filter);

            return await query.ToListAsync();
        }

        public async Task<Quote?> GetQuoteByIdAsync(Guid quoteId)
        {
            return await _context.Quotes
                .Include(q => q.CustomerOrders)
                .Include(q => q.QuoteDetails)
                    .ThenInclude(qd => qd.ElectricVehicleVersion)
                        .ThenInclude(v => v.Model)
                .Include(q => q.QuoteDetails)
                    .ThenInclude(qd => qd.ElectricVehicleColor)
                .Include(q => q.QuoteDetails)
                    .ThenInclude(qd => qd.Promotion)
                .Include(q => q.Dealer)
                    .ThenInclude(d => d.Warehouse)
                .Include(q => q.Dealer)
                .FirstOrDefaultAsync(q => q.Id == quoteId);
        }

        public async Task<bool> IsQuoteExistByIdAsync(Guid quoteId)
        {
            return await _context.Quotes.AnyAsync(q => q.Id == quoteId);
        }
    }
}
