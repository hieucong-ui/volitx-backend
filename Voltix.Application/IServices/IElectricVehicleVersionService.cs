using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.ElectricVehicleVersion;
using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.IServices
{
    public interface IElectricVehicleVersionService
    {
        Task<ResponseDTO> GetAllVersionsAsync();
        Task<ResponseDTO> GetVersionByIdAsync(Guid versionId);
        Task<ResponseDTO> GetVersionByNameAsync(string versionName);
        Task<ResponseDTO> CreateVersionAsync(CreateElectricVehicleVersionDTO createElectricVehicleVersionDTO);
        Task<ResponseDTO> UpdateVersionAsync(Guid versionId, UpdateElectricVehicleVersionDTO updateElectricVehicleVersionDTO);
        Task<ResponseDTO> DeleteVersionAsync(Guid versionId);
        Task<ResponseDTO> GetAllAvailableVersionsForBookingByModelIdAsync(Guid modelId);
        Task<ResponseDTO> GetAllAvailableVersionsByModelIdAsync(Guid modelId);



    }
}
