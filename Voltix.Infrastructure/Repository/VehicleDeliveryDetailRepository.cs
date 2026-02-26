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
    public class VehicleDeliveryDetailRepository : Repository<VehicleDeliveryDetail>, IVehicleDeliveryDetailRepository
    {
        public readonly ApplicationDbContext _context;
        public VehicleDeliveryDetailRepository(ApplicationDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<VehicleDeliveryDetail?> GetVehicleDeliveryDetailById(Guid detailId)
        {
            return await _context.VehicleDeliveryDetails.FirstOrDefaultAsync(vdd => vdd.Id == detailId);
        }
    }
}
