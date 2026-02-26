using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.EVTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.IServices
{
    public interface IEVTemplateService
    {
        Task<ResponseDTO> CreateEVTemplateAsync(CreateEVTemplateDTO createEVTemplateDTO);
        Task<ResponseDTO> GetVehicleTemplateByIdAsync(Guid EVTemplateId);
        Task<ResponseDTO> GetAllVehicleTemplateAsync(int pageNumber, int pageSize, string? search, Guid? templateId, decimal? minPrice, decimal? maxPrice, bool sortByPriceAsc, CancellationToken ct);
        Task<ResponseDTO> UpdateEVTemplateAsync(Guid EVTemplateId, UpdateEVTemplateDTO updateEVTemplateDTO);
        Task<ResponseDTO> DeleteEVTemplateAsync(Guid EVTemplateId);
        Task<ResponseDTO> GetTemplatesByVersionAndColorAsync(Guid versionId,Guid colorId);
    }
}
