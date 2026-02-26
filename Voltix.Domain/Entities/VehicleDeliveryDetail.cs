using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.Entities
{
    public class VehicleDeliveryDetail
    {
        public Guid Id { get; set; }
        public Guid VehicleDeliveryId { get; set; }
        public Guid ElectricVehicleId { get; set; }
        public DeliveryVehicleStatus Status { get; set; }
        public string? Note { get; set; }

        public VehicleDelivery VehicleDelivery { get; set; } = null!;
        public ElectricVehicle ElectricVehicle { get; set; } = null!;
    }
}
