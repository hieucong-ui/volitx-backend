using Microsoft.EntityFrameworkCore;
using Voltix.Domain.Entities;
using Voltix.Domain.Enums;
using Voltix.Infrastructure.Context;
using Voltix.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Infrastructure.Repository
{
    public class ElectricVehicleColorRepository : Repository<ElectricVehicleColor>, IElectricVehicleColorRepository
    {
        public readonly ApplicationDbContext _context;
        public ElectricVehicleColorRepository(ApplicationDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<ElectricVehicleColor>> GetAllColorsByModelIdAndVersionIdAsync(Guid modelId, Guid versionId)
        {
                var colors = await _context.ElectricVehicleTemplates
                    .Include(t => t.Color)
                    .Include(t => t.Version)
                    .Where(t => t.Version.ModelId == modelId
                             && t.VersionId == versionId)
                    .Select(t => t.Color)
                    .ToListAsync();
            return colors;
        }

        public async Task<List<ElectricVehicleColor?>> GetAvailableColorsByModelIdAndVersionIdAsync(Guid modelId, Guid versionId)
        {
            var colors = await _context.ElectricVehicles
                .Include(ev => ev.ElectricVehicleTemplate)
                .Where(ev => ev.ElectricVehicleTemplate.Version.ModelId == modelId
                     && ev.ElectricVehicleTemplate.VersionId == versionId
                     && ev.Status == ElectricVehicleStatus.Available
                     && ev.Warehouse.EVCInventoryId != null)
                .Select(ev => ev.ElectricVehicleTemplate.Color)
                .Distinct()
                .ToListAsync();
            return colors;
        }

        public async Task<ElectricVehicleColor?> GetByCodeAsync(string colorCode)
        {
            return await _context.ElectricVehicleColors
                .FirstOrDefaultAsync(c => c.ColorCode == colorCode);
        }

        public async Task<ElectricVehicleColor?> GetByIdsAsync(Guid colorId)
        {
            return await _context.ElectricVehicleColors
                .FirstOrDefaultAsync(c => c.Id == colorId);
        }

        public Task<ElectricVehicleColor?> GetByNameAsync(string colorName)
        {
            return _context.ElectricVehicleColors
                .FirstOrDefaultAsync(c => c.ColorName == colorName);
        }

        public async Task<bool> IsColorExistsByCode(string colorCode)
        {
            return await _context.ElectricVehicleColors
                .AnyAsync(c => c.ColorCode == colorCode);
        }

        public async Task<bool> IsColorExistsById(Guid colorId)
        {
            return await _context.ElectricVehicleColors
                .AnyAsync(c => c.Id == colorId);
        }

        public async Task<bool> IsColorExistsByName(string colorName)
        {
             return await _context.ElectricVehicleColors
                .AnyAsync(c => c.ColorName == colorName);
        }
    }
}
