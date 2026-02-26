using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.ElectricVehicleColor;
using Voltix.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.IServices
{
    public interface IElectricVehicleColorService
    {
        Task<ResponseDTO> GetAllColorsAsync();
        Task<ResponseDTO> GetAvailableColorsByModelIdAndVersionIdAsync(Guid modelId, Guid versionId);
        Task<ResponseDTO> GetColorByIdAsync(Guid colorId);
        Task<ResponseDTO> CreateColorAsync(CreateElectricVehicleColorDTO createElectricVehicleColorDTO);
        Task<ResponseDTO> UpdateColorAsync(Guid colorId, UpdateElectricVehicleColor updateElectricVehicleColor);
        Task<ResponseDTO> DeleteColorAsync(Guid colorId);
        Task<ResponseDTO> GetAllColorsByModelIdAndVersionIdAsync(Guid modelId, Guid versionId);
        Task<ResponseDTO> GetColorByNameAsync(string colorName);
        Task<ResponseDTO> GetColorByCodeAsync(string colorCode);
        
    }
}
