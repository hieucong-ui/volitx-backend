using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.DealerDebt;
using Voltix.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.IServices
{
    public interface IDealerDebtTransactionService
    {
        Task<ResponseDTO> CraeteDealerDebtTransaction(CreateDealerDebtTransactionDTO dealerDebtTransactionDTO, CancellationToken ct);
        Task<ResponseDTO<List<DealerDebtTransaction>>> GetAll(Guid dealerId, DateTime fromUtc, DateTime toUtc, int pageNumber, int pageSize, CancellationToken ct);
    }
}
