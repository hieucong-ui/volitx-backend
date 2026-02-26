using Voltix.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Infrastructure.IRepository
{
    public interface IElectricVehicleVersionRepository : IRepository<ElectricVehicleVersion>
    {
        Task<bool> IsVersionExistsById(Guid versionId);
        Task<bool> IsVersionExistsByName(string versionName);
        Task<ElectricVehicleVersion?> GetByIdsAsync(Guid versionId);
        Task<ElectricVehicleVersion?> GetByNameAsync(string versionName);
        Task<List<ElectricVehicleVersion>> GetAllVersionsByModelIdAsync(Guid modelId);


    }
}
