using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.CustomerOrder;
using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.IServices
{
    public interface ICustomerOrderService
    {
        Task<ResponseDTO> CreateCustomerOrderAsync(ClaimsPrincipal user, CreateCustomerOrderDTO createCustomerOrderDTO, CancellationToken ct);
        Task<ResponseDTO> GetAllCustomerOrders(ClaimsPrincipal userClaim, int pageNumber, int pageSize, OrderStatus? orderStatus, CancellationToken ct);
        Task<ResponseDTO> CancelCustomerOrderAsync(Guid customerOrderId, CancellationToken ct);
        Task<ResponseDTO> PayDeposit(Guid customerOrderId, bool? isCash, CancellationToken ct);
        Task<ResponseDTO> CustomerConfirm(Guid customerOrderId, string email, bool isAccept, CancellationToken ct);
        Task<ResponseDTO> PayCustomerOrder(ClaimsPrincipal userClaim, ConfirmCustomerOrderDTO confirmCustomerOrderDTO, CancellationToken ct);
        Task<ResponseDTO> AutoCancelExpiredDepositOrders(CancellationToken ct);
        Task<ResponseDTO> AutoCancelExpiredPendingOrders(CancellationToken ct);
    }
}
