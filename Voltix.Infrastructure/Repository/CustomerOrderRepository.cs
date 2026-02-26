using Microsoft.EntityFrameworkCore;
using Voltix.Domain.Entities;
using Voltix.Domain.Enums;
using Voltix.Infrastructure.Context;
using Voltix.Infrastructure.IRepository;

namespace Voltix.Infrastructure.Repository
{
    public class CustomerOrderRepository : Repository<CustomerOrder>, ICustomerOrderRepository
    {
        private readonly ApplicationDbContext _context;
        public CustomerOrderRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public int GenerateOrderNumber()
        {
            return _context.CustomerOrders.Count() + 1;
        }

        public async Task<CustomerOrder?> GetByIdAsync(Guid customerOrderId)
        {
            return await _context.CustomerOrders
                .Include(c => c.Quote)
                    .ThenInclude(q => q.QuoteDetails)
                        .ThenInclude(qd => qd.ElectricVehicleVersion)
                    .ThenInclude(ev => ev.Model)
                .Include(c => c.Quote)
                    .ThenInclude(q => q.QuoteDetails)
                        .ThenInclude(qd => qd.ElectricVehicleColor)
                .Include(c => c.Quote)
                    .ThenInclude(q => q.Dealer)
                        .ThenInclude(d => d.Manager)
                .Include(c => c.Quote)
                    .ThenInclude(q => q.Dealer)
                        .ThenInclude(d => d.Warehouse)
                .Include(c => c.Customer)
                .Include(c => c.OrderDetails)
                .FirstOrDefaultAsync(c => c.Id == customerOrderId);
        }

        public async Task<bool>? IsExistByIdAsync(Guid id)
        {
            return await _context.CustomerOrders.AnyAsync(c => c.Id == id);
        }

        public async Task<CustomerOrder?> GetByOrderNoAsync(int customerOrderNo)
        {
            return await _context.CustomerOrders
                .AsTracking()
                .Include(co => co.OrderDetails)
                .Include(co => co.Quote)
                .FirstOrDefaultAsync(c => c.OrderNo == customerOrderNo);
        }

        public async Task<CustomerOrder?> GetByEContractId(Guid eContractId, CancellationToken ct)
        {
            return await _context.CustomerOrders
                .Include(co => co.EContracts)
                .Include(co => co.Quote)
                .Include(co => co.Customer)
                .FirstOrDefaultAsync(co => co.EContracts!.Any(ec => ec.Id == eContractId), ct);
        }

        public async Task<List<CustomerOrder>?> GetAllCustomerOrderDeposit(CancellationToken ct)
        {
            return await _context.CustomerOrders
                .Where(co => co.Status == OrderStatus.Depositing || co.Status == OrderStatus.RemainingPending || co.Status == OrderStatus.RemainingConfimmed)
                .Include(co => co.Quote)
                    .ThenInclude(q => q.Dealer)
                .Include(co => co.Customer)
                .ToListAsync(ct);
        }

        public async Task<List<CustomerOrder>?> GetAllCustomerOrderPending(CancellationToken ct)
        {
            return await _context.CustomerOrders
                .Where(co => co.Status == OrderStatus.FullPending ||
                        co.Status == OrderStatus.DepositPending ||
                        co.Status == OrderStatus.ConfirmPending)
                .Include(co => co.Quote)
                    .ThenInclude(q => q.Dealer)
                .Include(co => co.Customer)
                .ToListAsync(ct);
        }
    }
}
