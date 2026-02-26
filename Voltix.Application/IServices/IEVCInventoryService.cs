using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.EVCInventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.IServices
{
    public interface IEVCInventoryService
    {
        Task<ResponseDTO> CreateEVCInventoryAsync(CreateEVCInventoryDTO cre);
        Task<ResponseDTO> DeleteEVCInventoryAsync(Guid evcInventoryId);
        Task<ResponseDTO> GetAllEVCInventoriesAsync();
        Task<ResponseDTO> GetEVCInventoryByIdAsync(Guid evcInventoryId);
        Task<ResponseDTO> UpdateEVCInventoryAsync(Guid evcInventoryId , UpdateEVCInventory updateEVCInventory);
    }
}
