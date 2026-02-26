using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.EContractTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.IServices
{
    public interface IEContractTemplateService
    {
        Task<ResponseDTO> CreateEContractTemplateAsync(CreateEContractTemplateDTO templateDTO, CancellationToken ct);
        Task<ResponseDTO> GetEContractTemplateByIdAsync(Guid eContractTemplateId, CancellationToken ct);
        Task<ResponseDTO> GetAll(int? pageNumber, int? pageSize, CancellationToken ct);
        Task<ResponseDTO> UpdateEcontractTemplateAsync(string code, UpdateEContractTemplateDTO templateDTO, CancellationToken ct);
    }
}
