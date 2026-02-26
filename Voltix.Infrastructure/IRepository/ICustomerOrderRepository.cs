using Voltix.Domain.Entities;
using Voltix.Domain.Enums;

namespace Voltix.Infrastructure.IRepository
{
    public interface ICustomerOrderRepository : IRepository<CustomerOrder>
    {
        Task<CustomerOrder?> GetByIdAsync(Guid customerOrderId);
        Task<bool>? IsExistByIdAsync(Guid id);
        Task<CustomerOrder?> GetByOrderNoAsync(int customerOrderNo);
        int GenerateOrderNumber();
        Task<CustomerOrder?> GetByEContractId(Guid eContractId, CancellationToken ct);
        Task<List<CustomerOrder>?> GetAllCustomerOrderDeposit(CancellationToken ct);
        Task<List<CustomerOrder>?> GetAllCustomerOrderPending(CancellationToken ct);
    }
}
