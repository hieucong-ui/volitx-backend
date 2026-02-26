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
    public class ElectricVehicleModelRepository : Repository<ElectricVehicleModel>, IElectricVehicleModelRepository
    {
        public readonly ApplicationDbContext _context;
        public ElectricVehicleModelRepository(ApplicationDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<ElectricVehicleModel>> GetAllWithVersionAsync()
        {
            return await _context.ElectricVehicleModels
                .Include(m => m.Versions)
                .Where(m => m.IsActive && m.Versions.Any(v => v.IsActive == true))
                .ToListAsync();
        }

        public async Task<ElectricVehicleModel?> GetByIdsAsync(Guid modelId)
        {
            return await _context.ElectricVehicleModels
                .FirstOrDefaultAsync(m => m.Id == modelId);
        }

        public async Task<ElectricVehicleModel?> GetByNameAsync(string modelName)
        {
            return await _context.ElectricVehicleModels
                .FirstOrDefaultAsync(m => m.ModelName == modelName);
        }

        public async Task<bool> IsModelExistsById(Guid modelId)
        {
            return await _context.ElectricVehicleModels
                .AnyAsync(m => m.Id == modelId);
        }

        public async Task<bool> IsModelExistsByName(string modelName)
        {
            return await _context.ElectricVehicleModels
                .AnyAsync(m => m.ModelName == modelName);
        }
    }
}
