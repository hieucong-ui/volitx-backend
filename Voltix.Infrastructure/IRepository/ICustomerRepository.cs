using Voltix.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Infrastructure.IRepository
{
    public interface ICustomerRepository : IRepository<Customer>
    {
        Task<List<Customer?>> GetAllCustomerAsync();
        Task<Customer?> GetByIdAsync(Guid customerId);
        Task<bool> IsExistByIdAsync(Guid customerId);
        Task<Customer?> GetByEmailAync(string email);
        Task<Customer?> GetByPhoneNumber(string phoneNumber);
        Task<int> CountCustomerByDealerId(Guid dealerId, CancellationToken ct);
    }
}
