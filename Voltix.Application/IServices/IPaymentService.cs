using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.Payment;
using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.IServices
{
    public interface IPaymentService
    {
        Task<ResponseDTO> CreateVNPayLink(Guid customerOrderId, CancellationToken ct);
        Task<ResponseDTO> HandleVNPayIpn(VNPayIPNDTO ipnDTO, CancellationToken ct);
        Task<ResponseDTO> GetAllPaymentTransaction(ClaimsPrincipal userClaim, int pageNumber, int pageSize, TransactionStatus? status, CancellationToken ct);
    }
}
