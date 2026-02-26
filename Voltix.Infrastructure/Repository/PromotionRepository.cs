using Microsoft.EntityFrameworkCore;
using Voltix.Domain.Entities;
using Voltix.Infrastructure.Context;
using Voltix.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Infrastructure.Repository
{
    public class PromotionRepository : Repository<Promotion>, IPromotionRepository
    {
        public readonly ApplicationDbContext _context;
        public PromotionRepository(ApplicationDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Promotion?> GetPromotionByIdAsync(Guid? id)
        {
            return await _context.Promotions.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Promotion?> GetPromotionByNameAsync(string name)
        {
            return await _context.Promotions.FirstOrDefaultAsync(p => p.Name == name);
        }

        public async Task<bool> IsExistPromotionByNameExceptAsync(string name , Guid exceptId)
        {
            return await _context.Promotions.AnyAsync(p => p.Name == name && p.Id != exceptId);
        }

        public async Task<bool> IsExistPromotionByNameAsync(string name)
        {
            return await _context.Promotions.AnyAsync(p => p.Name == name);
        }

        public async Task<Promotion?> GetActivePromotionByVersionIdAsync(Guid versionId)
        {
            return await _context.Promotions.FirstOrDefaultAsync(p => p.VersionId == versionId && p.IsActive);
        }

        public async Task<Promotion?> GetActivePromotionByModelIdAsync(Guid modelId)
        {
            return await _context.Promotions.FirstOrDefaultAsync(p => p.ModelId == modelId && p.IsActive);
        }
    }
}
