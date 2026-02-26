using Voltix.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Infrastructure.IRepository
{
    public interface IElectricVehicleRepository : IRepository<ElectricVehicle>
    {
        Task<bool> IsVehicleExistsById(Guid vehicleId);
        Task<bool> IsVehicleExistsByVIN(string vin);
        Task<ElectricVehicle?> GetByIdsAsync(Guid vehicleId);
        Task<ElectricVehicle?> GetByVINAsync(string vin);
        Task<List<ElectricVehicle>> GetAvailableVehicleForBookingByModelIdAsync(Guid modelId);
        Task<int> GetAvailableQuantityByModelVersionColorAsync(Guid modelId, Guid versionId, Guid colorId);
        Task<int> GetAvailableQuantityByVersionColorAsync(Guid versionId, Guid colorId);
        Task<List<ElectricVehicle>> GetAvailableVehicleByModelVersionColorAsync(Guid modelId, Guid versionId, Guid colorId);
        Task<List<ElectricVehicle>> GetDealerInventoryAsync(Guid dealerId);
        Task<List<ElectricVehicle>> GetAllVehicleWithDetailAsync();
        Task<List<ElectricVehicle>> GetAllEVCVehiclesWithDetailAsync();
        Task<List<ElectricVehicle>> GetAvailableVehicleByDealerAsync(Guid dealerId, Guid versionId, Guid colorId);
        Task<int> GetAvailableVehicleAsync(Guid dealerId, Guid versionId, Guid colorId);
        Task<ElectricVehicle?> GetByVersionColorAndWarehouseAsync(Guid versionId, Guid colorId, Guid warehouseId);
        Task<List<ElectricVehicle>> GetBookedVehicleByModelVersionColorAsync(Guid modelId, Guid versionId, Guid colorId);
        Task<List<ElectricVehicle>> GetPendingVehicleByModelVersionColorAsync(Guid modelId, Guid versionId, Guid colorId);
        Task<List<ElectricVehicle>> GetInTransitVehicleByModelVersionColorAsync(Guid modelId,Guid versionId,Guid colorId);
        Task<List<ElectricVehicle>> GetVehicleByQuantityWithOldestImportDateForDealerAsync(Guid versionId, Guid colorId, Guid warehouseId, int quantity);
        Task<int> CountDealerAvailableByVersionColorAsync(Guid dealerId, Guid versionId, Guid colorId, CancellationToken ct);
        Task<ElectricVehicle?> GetByVersionColorId(Guid VersionId, Guid ColorId);
        Task<int> CountAvailableByDealerAsync(Guid dealerId, CancellationToken ct);
        Task<int> GetTotalVehiclesInEVCAsync(CancellationToken ct);
        Task<ElectricVehicle?> GetFirstAvailableVehicleAsync(Guid versionId, Guid colorId, IEnumerable<Guid>? excludeVehicleIds, CancellationToken ct);
        Task<IReadOnlyDictionary<(Guid DealerId, Guid EVTemplateId), int>> GetInflowAsync(DateTime dayUtc, CancellationToken ct);
        Task<IReadOnlyDictionary<(Guid DealerId, Guid EVTemplateId), int>> GetOutflowAsync(DateTime dayUtc, CancellationToken ct);
        Task<IReadOnlyDictionary<(Guid DealerId, Guid EVTemplateId), int>> GetDealerOnHandStockAsync(CancellationToken ct);
    }
}
