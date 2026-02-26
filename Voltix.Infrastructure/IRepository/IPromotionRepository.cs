using Voltix.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Infrastructure.IRepository
{
    public interface IPromotionRepository :IRepository<Promotion>
    {
        Task<Promotion?> GetPromotionByIdAsync(Guid? id);
        Task<Promotion?> GetPromotionByNameAsync(string name);
        Task<bool> IsExistPromotionByNameExceptAsync(string name ,Guid expectId);
        Task<bool> IsExistPromotionByNameAsync(string name);
        Task<Promotion?> GetActivePromotionByVersionIdAsync(Guid versionId);
        Task<Promotion?> GetActivePromotionByModelIdAsync(Guid modelId);

    }
}
