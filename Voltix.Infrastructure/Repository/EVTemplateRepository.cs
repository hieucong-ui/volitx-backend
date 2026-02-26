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
    public class EVTemplateRepository : Repository<ElectricVehicleTemplate>, IEVTemplateRepository
    {
        private readonly ApplicationDbContext _context;
        public EVTemplateRepository(ApplicationDbContext context) : base(context) 
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<(Guid DealerId, Guid EVTemplateId)>> GetActiveDealerTemplatePairsAsync(CancellationToken ct)
        {
            return await _context.ElectricVehicles
                .Where(ev => ev.Warehouse != null && ev.Warehouse.DealerId != null)
                .Select(ev => new { DealerId = ev.Warehouse!.DealerId, ev.ElectricVehicleTemplateId })
                .Distinct()
                .Select(x => new ValueTuple<Guid, Guid>(x.DealerId!.Value, x.ElectricVehicleTemplateId))
                .ToListAsync(ct);
        }

        public async Task<ElectricVehicleTemplate?> GetByIdAsync(Guid EVTemplateId)
        {
            return await _context.ElectricVehicleTemplates
                .Include(evt => evt.EVAttachments)
                .FirstOrDefaultAsync(evt => evt.Id == EVTemplateId);
        }

        public async Task<ElectricVehicleTemplate?> GetByVersionColorAndWarehouseAsync(Guid versionId, Guid colorId, Guid warehouseId)
        {
            return await _context.ElectricVehicleTemplates
                .Include(evt => evt.ElectricVehicles)
                    .ThenInclude(ev => ev.Warehouse)
                .FirstOrDefaultAsync(t =>t.VersionId == versionId 
                                    && t.ColorId == colorId 
                                    && t.ElectricVehicles.Any(ev => ev.WarehouseId == warehouseId 
                                    && ev.Warehouse.WarehouseType == WarehouseType.Dealer));
        }

        public async Task<ElectricVehicleTemplate?> GetTemplatesByVersionAndColorAsync(Guid versionId, Guid colorId)
        {
            return await _context.ElectricVehicleTemplates
                .Include(evt => evt.Version)
                    .ThenInclude(v => v.Model)
                .Include(evt => evt.Color)
                .Where(evt => evt.VersionId == versionId && evt.ColorId == colorId).FirstOrDefaultAsync();
        }

        public async Task<bool>? IsEVTemplateExistsById(Guid EVTemplateId)
        {
            return await _context.ElectricVehicleTemplates.AnyAsync(evt => evt.Id == EVTemplateId);
        }
    }
}
