using EVManagementSystem.Application.DTO.EContract;
using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.EContract;
using Voltix.Domain.Entities;
using Voltix.Domain.Enums;
using Voltix.Domain.ValueObjects;
using System.Security.Claims;

namespace Voltix.Application.IServices
{
    public interface IEContractService
    {
        Task<ResponseDTO<GetAccessTokenDTO>> GetAccessTokenAsync();
        Task<ProcessLoginInfoDto> GetAccessTokenAsyncByCode(string processCode, CancellationToken ct = default);
        Task<byte[]> DownloadAsync(string url);

        Task<ResponseDTO> SignProcess(string token, VnptProcessDTO vnptProcessDTO, CancellationToken ct);
        Task<HttpResponseMessage> GetPreviewResponseAsync(string token, string? rangeHeader = null, CancellationToken ct = default);
        Task<ResponseDTO> CreateEContractAsync(ClaimsPrincipal userClaim, Guid eContractId, CancellationToken ct);
        Task<VnptResult<VnptSmartCAResponse>> AddSmartCA(AddNewSmartCADTO addNewSmartCADTO);
        Task<VnptResult<VnptFullUserData>> GetSmartCAInformation(int userId);
        Task<VnptResult<VnptSmartCAResponse>> UpdateSmartCA(UpdateSmartDTO updateSmartDTO);
        Task<VnptResult<UpdateEContractResponse>> UpdateEContract(ClaimsPrincipal userClaim, UpdateEContractDTO updateEContractDTO, CancellationToken ct);
        Task<ResponseDTO<EContract>> GetAllEContractList(ClaimsPrincipal userClaim, int? pageNumber, int? pageSize, EContractStatus eContractStatus = default, 
            EcontractType econtractType = default, CancellationToken ct = default);
        Task<ResponseDTO> CreateDraftEContractAsync(ClaimsPrincipal userClaim, CreateDealerDTO createDealerDTO, CancellationToken ct);
        Task<VnptResult<VnptDocumentDto>> GetVnptEContractByIdAsync(string eContractId, CancellationToken ct);
        Task<ResponseDTO<EContract>> GetEContractByIdAsync(string eContractId, CancellationToken ct);
        Task<VnptResult<GetEContractResponse<DocumentListItemDto>>> GetAllVnptEContractList(int? pageNumber, int? pageSize, EContractStatus eContractStatus);
        Task<ResponseDTO> CreateBookingEContractAsync(ClaimsPrincipal userClaim, Guid bookingId, CancellationToken ct);
        Task<ResponseDTO> DeleteEContractDraft(Guid EContractId, CancellationToken ct);
        Task<VnptResult<DeleteSmartCAResponse>> DeleteSmartCA(DeleteSmartCARequest deleteSmartCARequest);
        Task<ResponseDTO> CreateDepositEContractConfirm(Guid customerOderId, CancellationToken ct);
        Task<ResponseDTO> ReadyCustomerOrderEcontract(Guid eContractId, CancellationToken ct);
        Task<ResponseDTO> CreatePayFullConfirmationEContract(Guid customerOderId, CancellationToken ct);
        Task<ResponseDTO> ConfirmBookingEVEContract(ClaimsPrincipal userClaim, Guid EContractId, CancellationToken ct);
        Task<ResponseDTO> CreateEContractInvoiceConfirmBookingEV(Guid customerOrderId, CancellationToken ct);
    }
}
