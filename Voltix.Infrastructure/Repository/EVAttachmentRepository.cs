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
    public class EVAttachmentRepository : Repository<EVAttachment>, IEVAttachmentRepository
    {
        private readonly ApplicationDbContext _context;
        public EVAttachmentRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public List<EVAttachment>? GetAttachmentsByElectricVehicleTemplateId(Guid electricVehicleTemplateId)
        {
            return _context.EVAttachments
                .Where(att => att.ElectricVehicleTemplateId == electricVehicleTemplateId).ToList();
        }
    }
}
