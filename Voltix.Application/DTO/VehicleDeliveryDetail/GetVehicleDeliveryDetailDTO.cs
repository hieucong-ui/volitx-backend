using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.VehicleDeliveryDetail
{
    public class GetVehicleDeliveryDetailDTO
    {
        public Guid Id { get; set; }
        public Guid VehicleDeliveryId { get; set; }
        public string? VIN { get; set; }
        public Guid ElectricVehicleId { get; set; }
        public DeliveryVehicleStatus Status { get; set; }
        public string? Note { get; set; }

    }
}
