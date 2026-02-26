using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.Quote;
using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.IServices
{
    public interface IQuoteService
    {
        Task<ResponseDTO> GetAllAsync(ClaimsPrincipal user, int pageNumber, int pageSize, Guid? modelId,Guid? versionId,Guid? colorId, QuoteStatus? status, bool onlyToday = false, CancellationToken ct = default);
        Task<ResponseDTO> GetQuoteByIdAsync(ClaimsPrincipal user , Guid id);
        Task<ResponseDTO> CreateQuoteAsync(ClaimsPrincipal user , CreateQuoteDTO createQuoteDTO);
        Task<ResponseDTO> UpdateQuoteStatusAsync(ClaimsPrincipal user , Guid id, QuoteStatus newStatus);
        Task<ResponseDTO> UpdateExpiredQuoteAsync(ClaimsPrincipal user , CancellationToken ct);

    }
}
