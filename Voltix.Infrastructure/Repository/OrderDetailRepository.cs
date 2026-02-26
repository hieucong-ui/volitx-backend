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
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {
        private readonly ApplicationDbContext _context;
        public OrderDetailRepository(ApplicationDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<OrderDetail>?> GetAllByCustomerOrderId(Guid customerOrderId, CancellationToken ct)
        {
            return await _context.OrderDetails
                .Include(od => od.ElectricVehicle)
                .Where(od => od.CustomerOrderId == customerOrderId)
                .ToListAsync(ct);
        }
    }
}
