using Voltix.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Infrastructure.IRepository
{
    public interface IQuoteRepository : IRepository<Quote>
    {
        Task<Quote?> GetQuoteByIdAsync(Guid quoteId);
        Task<bool> IsQuoteExistByIdAsync(Guid quoteId);
        Task<List<Quote>> GetAllQuotesWithDetailAsync(Expression<Func<Quote, bool>>? filter = null);
        Task<int> CountByDealerIdAsync(Guid dealerId, CancellationToken ct);
    }
}
