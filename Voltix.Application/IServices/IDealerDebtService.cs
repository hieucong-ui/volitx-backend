using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.DealerDebt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.IServices
{
    public interface IDealerDebtService
    {
        Task<ResponseDTO> AddPurchaseForDealerAsync(Guid dealerId, RecordDebtDTO debtDTO, CancellationToken ct);
        Task<ResponseDTO> AddPaymentForDealerAsync(Guid dealerId, RecordPaymentDTO paymentDTO, CancellationToken ct);
        Task<ResponseDTO> AddCommissionForDealerAsync(Guid dealerId, RecordCommissionDTO dto, CancellationToken ct);
        Task<ResponseDTO> GetDealerDebtBalanceAtQuarterNow(ClaimsPrincipal userClaim, Guid? dealerId, CancellationToken ct);
        Task<ResponseDTO> GetDealerDebtDetails(ClaimsPrincipal userClaim, Guid? dealerId, DateTime fromDateUtc, DateTime toDateUtc, int pageNumber, int pageSize, CancellationToken ct);
    }
}
