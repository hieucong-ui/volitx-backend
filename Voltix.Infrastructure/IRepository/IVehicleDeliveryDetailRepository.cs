using Voltix.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Infrastructure.IRepository
{
    public interface IVehicleDeliveryDetailRepository : IRepository<VehicleDeliveryDetail>
    {
        Task<VehicleDeliveryDetail?> GetVehicleDeliveryDetailById(Guid detailId);
    }
}
