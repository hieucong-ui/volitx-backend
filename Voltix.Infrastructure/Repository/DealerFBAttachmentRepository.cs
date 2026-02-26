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
    public class DealerFBAttachmentRepository : Repository<DealerFBAttachment>, IDealerFBAttachmentRepository
    {
        public readonly ApplicationDbContext _context;
        public DealerFBAttachmentRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public List<DealerFBAttachment>? GetAttachmentsByDealerFbId(Guid dealerFbId)
        {
            return _context.DealerFBAttachments.Where(a => a.DealerFeedBackId == dealerFbId).ToList();
        }
    }
}
