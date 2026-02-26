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
    internal class CustomerFBAttachRepository : Repository<CustomerFBAttachment>, ICustomerFBAttachRepository
    {
        private readonly ApplicationDbContext _context;
        public CustomerFBAttachRepository(ApplicationDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public List<CustomerFBAttachment>? GetAttachmentsByCustomerFbId(Guid customerFbId)
        {
            return _context.CustomerFBAttachments
                .Where(attach => attach.CustomerFeedBackId == customerFbId)
                .ToList();
        }
    }
}
