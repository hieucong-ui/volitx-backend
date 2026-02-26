using Microsoft.EntityFrameworkCore;
using Voltix.Domain.Entities;
using Voltix.Infrastructure.Context;
using Voltix.Infrastructure.IRepository;

namespace Voltix.Infrastructure.Repository
{
    public class CustomerRepository : Repository<Customer>, ICustomerRepository
    {
        private readonly ApplicationDbContext _context;
        public CustomerRepository(ApplicationDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<int> CountCustomerByDealerId(Guid dealerId, CancellationToken ct)
        {
            return await _context.Customers
                .Where(c => c.Dealers.Any(d => d.Id == dealerId))
                .CountAsync(ct);
        }

        public async Task<List<Customer>> GetAllCustomerAsync()
        {
            return await _context.Customers
                .Include(c => c.Dealers)
                .ToListAsync();


        }

        public async Task<Customer?> GetByEmailAync(string email)
        {
            return await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
        }

        public async Task<Customer?> GetByIdAsync(Guid customerId)
        {
            return await _context.Customers.FirstOrDefaultAsync(c => c.Id == customerId);
        }

        public async Task<Customer?> GetByPhoneNumber(string phoneNumber)
        {
            return await _context.Customers.FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);
        }

        public async Task<bool> IsExistByIdAsync(Guid customerId)
        {
            return await _context.Customers.AnyAsync(c => c.Id == customerId);
        }


    }
}
