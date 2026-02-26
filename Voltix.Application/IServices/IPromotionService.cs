using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.Promotion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.IServices
{
    public interface IPromotionService
    {
        Task<ResponseDTO> GetPromotionByIdAsync(Guid promotionId);
        Task<ResponseDTO> GetAllAsync();
        Task<ResponseDTO> CreatePromotionAsync(CreatePromotionDTO createPromotionDTO);
        Task<ResponseDTO> UpdatePromotionAsync(Guid promotionId,UpdatePromotionDTO updatePromotionDTO);
        Task<ResponseDTO> DeletePromotionAsync(Guid promotionId);
        Task<ResponseDTO> GetPromotionsForQuoteAsync(Guid? modelId, Guid? versionId);
        Task<ResponseDTO> GetPromotionByNameAsync(string name);
    }
}
