using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.Customer;
using System.Security.Claims;

namespace Voltix.Application.IServices
{
    public interface ICustomerService
    {
        Task<ResponseDTO> CreateCustomerAsync(ClaimsPrincipal user,CreateCustomerDTO createCustomerDTO);
        Task<ResponseDTO> GetAllCustomerAsync(ClaimsPrincipal user, int pageNumber, int pageSize, string? search, CancellationToken ct);
        Task<ResponseDTO> GetCustomerByIdAsync(ClaimsPrincipal user,Guid customerId);
        Task<ResponseDTO> UpdateCustomerAsync(ClaimsPrincipal user,Guid customerId, UpdateCustomerDTO updateCustomerDTO);
    }
}
