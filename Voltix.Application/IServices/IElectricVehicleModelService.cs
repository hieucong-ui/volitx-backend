using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.ElectricVehicleModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.IServices
{
    public interface IElectricVehicleModelService
    {
        Task<ResponseDTO> GetAllModelsAsync();
        Task<ResponseDTO> GetAllWithVersionAsync();
        Task<ResponseDTO> GetModelByIdAsync(Guid modelId);
        Task<ResponseDTO> CreateModelAsync(CreateElectricVehicleModelDTO createElectricVehicleModelDTO);
        Task<ResponseDTO> UpdateModelAsync(Guid modelId, UpdateElectricVehicleModelDTO updateElectricVehicleModelDTO);
        Task<ResponseDTO> DeleteModelAsync(Guid modelId);
        Task<ResponseDTO> GetModelByNameAsync(string modelName);
    }
}
