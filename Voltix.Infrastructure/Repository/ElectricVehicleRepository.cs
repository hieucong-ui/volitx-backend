using Microsoft.EntityFrameworkCore;
using Voltix.Domain.Entities;
using Voltix.Domain.Enums;
using Voltix.Infrastructure.Context;
using Voltix.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Voltix.Infrastructure.Repository
{
    public class ElectricVehicleRepository : Repository<ElectricVehicle>, IElectricVehicleRepository
    {
        public readonly ApplicationDbContext _context;
        public ElectricVehicleRepository(ApplicationDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<ElectricVehicle?> GetByVersionColorId(Guid VersionId, Guid ColorId)
        {
            return await _context.ElectricVehicles
                .Include(ev => ev.ElectricVehicleTemplate)
                .FirstOrDefaultAsync(ev => ev.ElectricVehicleTemplate.VersionId == VersionId
                                        && ev.ElectricVehicleTemplate.ColorId == ColorId);
        }

        public async Task<List<ElectricVehicle>> GetAllEVCVehiclesWithDetailAsync()
        {
            return await _context.ElectricVehicles
                .Include(ev => ev.ElectricVehicleTemplate)
                    .ThenInclude(et => et.Version)
                        .ThenInclude(v => v.Model)
                .Include(ev => ev.ElectricVehicleTemplate.Color)
                .Include(ev => ev.Warehouse)
                .ToListAsync();
        }

        public async Task<List<ElectricVehicle>> GetAllVehicleWithDetailAsync()
        {
            return await _context.ElectricVehicles
                .Include(ev => ev.ElectricVehicleTemplate)
                    .ThenInclude(et => et.Version)
                        .ThenInclude(et => et.Model)
                .Include(ev => ev.ElectricVehicleTemplate.Color)
                .Where(ev => ev.Status == ElectricVehicleStatus.AtDealer)
                .ToListAsync();
        }

        public async Task<int> GetAvailableQuantityByModelVersionColorAsync(Guid modelId, Guid versionId, Guid colorId)
        {
            return await _context.ElectricVehicles
                .Include(ev => ev.ElectricVehicleTemplate)
                .Where(ev => ev.ElectricVehicleTemplate.Version.ModelId == modelId
                            && ev.ElectricVehicleTemplate.VersionId == versionId
                            && ev.ElectricVehicleTemplate.ColorId == colorId
                            && ev.Status == ElectricVehicleStatus.Available
                            && ev.Warehouse.EVCInventoryId != null)
                .CountAsync();
        }

        public Task<int> GetAvailableQuantityByVersionColorAsync(Guid versionId, Guid colorId)
        {
            return _context.ElectricVehicles
                .Include(ev => ev.ElectricVehicleTemplate)
                .Where(ev => ev.ElectricVehicleTemplate.VersionId == versionId
                             && ev.ElectricVehicleTemplate.ColorId == colorId
                             && ev.Status == ElectricVehicleStatus.Available
                             && ev.Warehouse.EVCInventoryId != null)
                .CountAsync();
        }
        // Count vehicle in dealer 's inventory
        public async Task<int> GetAvailableVehicleAsync(Guid dealerId, Guid versionId, Guid colorId)
        {
            return await _context.ElectricVehicles
                .Include(ev => ev.ElectricVehicleTemplate)
                .Where(ev => ev.Warehouse.DealerId == dealerId
                        && ev.ElectricVehicleTemplate.VersionId == versionId
                        && ev.ElectricVehicleTemplate.ColorId == colorId
                        && ev.Status == ElectricVehicleStatus.AtDealer)
                .CountAsync();
        }

        public async Task<List<ElectricVehicle>> GetAvailableVehicleByDealerAsync(Guid dealerId, Guid versionId, Guid colorId)
        {
            return await _context.ElectricVehicles
                .Include(ev => ev.ElectricVehicleTemplate)
                .ThenInclude(et => et.Version)
                .ThenInclude(v => v.Model)
                .Include(ev => ev.ElectricVehicleTemplate.Color)
                .Include(ev => ev.Warehouse)
                .Where(ev => ev.Warehouse.DealerId == dealerId
                        && ev.ElectricVehicleTemplate.VersionId == versionId
                        && ev.ElectricVehicleTemplate.ColorId == colorId
                        && ev.Status == ElectricVehicleStatus.AtDealer)
                .OrderBy(ev => ev.ImportDate)
                .ToListAsync();
        }

        public async Task<List<ElectricVehicle>> GetAvailableVehicleForBookingByModelIdAsync(Guid modelId)
        {
            return await _context.ElectricVehicles
                .Include(ev => ev.ElectricVehicleTemplate)
                .Where(ev => ev.ElectricVehicleTemplate.Version.ModelId == modelId
                     && ev.Status == ElectricVehicleStatus.Available
                     && ev.Warehouse.EVCInventoryId != null)
                .Include(ev => ev.ElectricVehicleTemplate.Version)
                .Include(ev => ev.ElectricVehicleTemplate.Color)
                .Include(ev => ev.Warehouse)
                .ToListAsync();
        }

        public async Task<List<ElectricVehicle>> GetAvailableVehicleByModelVersionColorAsync(Guid modelId, Guid versionId, Guid colorId)
        {
            return await _context.ElectricVehicles
                .Include(ev => ev.Warehouse)
                .Include(ev => ev.ElectricVehicleTemplate)
                .Where(ev => ev.ElectricVehicleTemplate.Version.ModelId == modelId
                             && ev.ElectricVehicleTemplate.VersionId == versionId
                             && ev.ElectricVehicleTemplate.ColorId == colorId
                             && ev.Status == ElectricVehicleStatus.Available
                             && ev.Warehouse.WarehouseType == WarehouseType.EVInventory)
                .OrderBy(ev => ev.ImportDate)
                .ToListAsync();
        }

        public async Task<ElectricVehicle?> GetByIdsAsync(Guid vehicleId)
        {
            return await _context.ElectricVehicles
                .Include(ev => ev.ElectricVehicleTemplate)
                .ThenInclude(et => et.Version)
                .ThenInclude(v => v.Model)
                .Include(ev => ev.ElectricVehicleTemplate.Color)
                .Include(ev => ev.Warehouse)
                .FirstOrDefaultAsync(v => v.Id == vehicleId);
        }

        public async Task<ElectricVehicle?> GetByVersionColorAndWarehouseAsync(Guid versionId, Guid colorId, Guid warehouseId)
        {
            return await _context.ElectricVehicles
                .Include(v => v.Warehouse)
                .Include(v => v.ElectricVehicleTemplate)
                .FirstOrDefaultAsync(v => v.ElectricVehicleTemplate.VersionId == versionId
                                       && v.ElectricVehicleTemplate.ColorId == colorId
                                       && v.Warehouse.Id == warehouseId);
        }

        public async Task<int> CountDealerAvailableByVersionColorAsync(Guid dealerId, Guid versionId, Guid colorId, CancellationToken ct)
        {
            return await _context.ElectricVehicles
                .Include(ev => ev.Warehouse)
                .Include(ev => ev.ElectricVehicleTemplate).ThenInclude(t => t.Version)
                .Include(ev => ev.ElectricVehicleTemplate).ThenInclude(t => t.Color)
                .Where(ev => ev.Warehouse.DealerId == dealerId
                          && ev.ElectricVehicleTemplate.VersionId == versionId
                          && ev.ElectricVehicleTemplate.ColorId == colorId
                          && ev.Status == ElectricVehicleStatus.AtDealer)
                .CountAsync(ct);
        }

        public async Task<ElectricVehicle?> GetByVINAsync(string vin)
        {
            return await _context.ElectricVehicles
                .FirstOrDefaultAsync(v => v.VIN == vin);
        }

        public async Task<List<ElectricVehicle>> GetDealerInventoryAsync(Guid dealerId)
        {
            return await _context.ElectricVehicles
                .Include(ev => ev.ElectricVehicleTemplate)
                    .ThenInclude(evt => evt.Version)
                        .ThenInclude(v => v.Model)
                .Include(ev => ev.ElectricVehicleTemplate.Color)
                .Include(ev => ev.Warehouse)
                .Where(ev => ev.Warehouse.DealerId == dealerId
                            && ev.Status == ElectricVehicleStatus.AtDealer)
                .ToListAsync();
        }

        public async Task<List<ElectricVehicle>> GetPendingVehicleByModelVersionColorAsync(Guid modelId, Guid versionId, Guid colorId)
        {
            return await _context.ElectricVehicles
                .Include(ev => ev.Warehouse)
                .Include(ev => ev.ElectricVehicleTemplate)
                .Where(ev => ev.ElectricVehicleTemplate.Version.ModelId == modelId
                             && ev.ElectricVehicleTemplate.VersionId == versionId
                             && ev.ElectricVehicleTemplate.ColorId == colorId
                             && ev.Status == ElectricVehicleStatus.Pending
                             && ev.Warehouse.WarehouseType == WarehouseType.EVInventory)
                .OrderBy(ev => ev.ImportDate)
                .ToListAsync();
        }

        public async Task<List<ElectricVehicle>> GetVehicleByQuantityWithOldestImportDateForDealerAsync(Guid versionId, Guid colorId, Guid warehouseId, int quantity)
        {
            return await _context.ElectricVehicles
                .Include(ev => ev.Warehouse)
                .Include(ev => ev.ElectricVehicleTemplate)
                .Where(ev => ev.WarehouseId == warehouseId
                     && ev.ElectricVehicleTemplate.VersionId == versionId
                     && ev.ElectricVehicleTemplate.ColorId == colorId
                     && ev.Status == ElectricVehicleStatus.AtDealer
                     && ev.Warehouse.WarehouseType == WarehouseType.Dealer)
                .OrderBy(ev => ev.ImportDate)
                .Take(quantity)
                .ToListAsync();
        }

        public async Task<bool> IsVehicleExistsById(Guid vehicleId)
        {
            return await _context.ElectricVehicles
                .AnyAsync(v => v.Id == vehicleId);
        }

        public async Task<bool> IsVehicleExistsByVIN(string vin)
        {
            return await _context.ElectricVehicles
                .AnyAsync(v => v.VIN == vin);
        }

        public async Task<List<ElectricVehicle>> GetBookedVehicleByModelVersionColorAsync(Guid modelId, Guid versionId, Guid colorId)
        {
            return await _context.ElectricVehicles
                .Include(ev => ev.Warehouse)
                .Include(ev => ev.ElectricVehicleTemplate)
                .Where(ev => ev.ElectricVehicleTemplate.Version.ModelId == modelId
                             && ev.ElectricVehicleTemplate.VersionId == versionId
                             && ev.ElectricVehicleTemplate.ColorId == colorId
                             && ev.Status == ElectricVehicleStatus.Booked
                             && ev.Warehouse.WarehouseType == WarehouseType.EVInventory)
                                .OrderBy(ev => ev.ImportDate)
                .ToListAsync();
        }

        public async Task<List<ElectricVehicle>> GetInTransitVehicleByModelVersionColorAsync(Guid modelId, Guid versionId, Guid colorId)
        {
            return await _context.ElectricVehicles
                .Include(ev => ev.Warehouse)
                .Include(ev => ev.ElectricVehicleTemplate)
                .Where(ev => ev.ElectricVehicleTemplate.Version.ModelId == modelId
                             && ev.ElectricVehicleTemplate.VersionId == versionId
                             && ev.ElectricVehicleTemplate.ColorId == colorId
                             && ev.Status == ElectricVehicleStatus.InTransit
                             && ev.Warehouse.WarehouseType == WarehouseType.EVInventory)
                                .OrderBy(ev => ev.ImportDate)
                .ToListAsync();
        }

        public async Task<ElectricVehicle?> GetFirstAvailableVehicleAsync(Guid versionId, Guid colorId, IEnumerable<Guid>? excludeVehicleIds, CancellationToken ct)
        {
            var query = _context.ElectricVehicles
                .Include(ev => ev.ElectricVehicleTemplate)
                .Where(ev => ev.Status == ElectricVehicleStatus.Available
                    && ev.ElectricVehicleTemplate.VersionId == versionId
                    && ev.ElectricVehicleTemplate.ColorId == colorId);
            if (excludeVehicleIds != null && excludeVehicleIds.Any())
            {
                query = query.Where(ev => !excludeVehicleIds.Contains(ev.Id));
            }

            return await query.OrderBy(ev => ev.ImportDate).FirstOrDefaultAsync(ct);
        }

        public Task<int> CountAvailableByDealerAsync(Guid dealerId, CancellationToken ct)
        {
            return _context.ElectricVehicles
        .Where(ev => ev.Warehouse.DealerId == dealerId && ev.Status == ElectricVehicleStatus.AtDealer)
        .CountAsync(ct);
        }

        public async Task<int> GetTotalVehiclesInEVCAsync(CancellationToken ct)
        {
            return await _context.ElectricVehicles
                .Include(ev => ev.Warehouse)
                .CountAsync(ev => ev.Warehouse.WarehouseType == WarehouseType.EVInventory, ct);
        }

        public async Task<IReadOnlyDictionary<(Guid DealerId, Guid EVTemplateId), int>> GetInflowAsync(DateTime dayUtc, CancellationToken ct)
        {
            var dayOnly = dayUtc.Date;
            var query =
                from vdd in _context.VehicleDeliveryDetails
                join vd in _context.VehicleDeliveries on vdd.VehicleDeliveryId equals vd.Id
                join b in _context.BookingEVs on vd.BookingEVId equals b.Id
                join ev in _context.ElectricVehicles on vdd.ElectricVehicleId equals ev.Id
                where vd.Status == DeliveryStatus.Confirmed
                      && vdd.Status == DeliveryVehicleStatus.Delivered
                      && vd.UpdateAt.HasValue && vd.UpdateAt.Value.Date == dayOnly
                group vdd by new { b.DealerId, ev.ElectricVehicleTemplateId } into g
                select new
                {
                    g.Key.DealerId,
                    g.Key.ElectricVehicleTemplateId,
                    Qty = g.Count()
                };

            var list = await query.AsNoTracking().ToListAsync(ct);

            return list.ToDictionary(x => (x.DealerId, x.ElectricVehicleTemplateId), x => x.Qty);
        }

        public async Task<IReadOnlyDictionary<(Guid DealerId, Guid EVTemplateId), int>> GetOutflowAsync(DateTime dayUtc, CancellationToken ct)
        {
            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

            var vnStart = DateTime.SpecifyKind(dayUtc.Date, DateTimeKind.Unspecified);
            var vnEnd = vnStart.AddDays(1);

            var utcStart = TimeZoneInfo.ConvertTimeToUtc(vnStart, vnTimeZone);
            var utcEnd = TimeZoneInfo.ConvertTimeToUtc(vnEnd, vnTimeZone);
            var query =
                from od in _context.OrderDetails
                join ev in _context.ElectricVehicles on od.ElectricVehicleId equals ev.Id
                join w in _context.Warehouses on ev.WarehouseId equals w.Id
                where od.CreatedAt >= utcStart && od.CreatedAt < utcEnd
                group od by new { w.DealerId, ev.ElectricVehicleTemplateId } into g
                select new
                {
                    g.Key.DealerId,
                    g.Key.ElectricVehicleTemplateId,
                    Qty = g.Count()
                };

            var list = await query.AsNoTracking().ToListAsync(ct);

            return list.ToDictionary(x => (x.DealerId!.Value, x.ElectricVehicleTemplateId), x => x.Qty);
        }

        public async Task<IReadOnlyDictionary<(Guid DealerId, Guid EVTemplateId), int>> GetDealerOnHandStockAsync(CancellationToken ct)
        {
            var validStatuses = new[]
            {
                ElectricVehicleStatus.AtDealer,
                ElectricVehicleStatus.DealerPending,
                ElectricVehicleStatus.DepositBooked
            };

            var query =
                from ev in _context.ElectricVehicles
                join w in _context.Warehouses on ev.WarehouseId equals w.Id
                where w.WarehouseType == WarehouseType.Dealer
                      && validStatuses.Contains(ev.Status)
                group ev by new { w.DealerId, ev.ElectricVehicleTemplateId } into g
                select new
                {
                    DealerId = g.Key.DealerId!.Value,
                    g.Key.ElectricVehicleTemplateId,
                    Qty = g.Count()
                };

            var list = await query.AsNoTracking().ToListAsync(ct);
            return list.ToDictionary(x => (x.DealerId, x.ElectricVehicleTemplateId), x => x.Qty);
        }
    }
}
