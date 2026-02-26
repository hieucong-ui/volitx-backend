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
    public class DealerFeedbackRepository : Repository<DealerFeedback>, IDealerFeedbackRepository
    {
        public readonly ApplicationDbContext _context;
        public DealerFeedbackRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<DealerFeedback>> GetAllDealerFeedbacksWithDetailAsync(CancellationToken ct)
        {
            return await _context.DealerFeedbacks
                .Include(dfb => dfb.Dealer)
                .Include(dfb => dfb.DealerFBAttachments)
                .OrderByDescending(dfb => dfb.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<DealerFeedback?> GetFeedbackByIdAsync(Guid id)
        {
            return await _context.DealerFeedbacks
                .Include(dfb => dfb.Dealer)
                .Include(dfb => dfb.DealerFBAttachments)
                .FirstOrDefaultAsync(dfb => dfb.Id == id);
        }

        public async Task<List<DealerFeedback>> GetFeedbacksByDealerIdAsync(Guid dealerId)
        {
            return await _context.DealerFeedbacks
                .Where(dfb => dfb.DealerId == dealerId)
                .ToListAsync();
        }
    }
}
