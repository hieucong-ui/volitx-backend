using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.ElectricVehicle
{
    public class UpdateElectricVehicleDTO
    {
        public string? VIN { get; set; } = null!;
        public ElectricVehicleStatus? Status { get; set; }
        public DateTime? ManufactureDate { get; set; }
        public DateTime? ImportDate { get; set; }
        public DateTime? WarrantyExpiryDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public DateTime? DealerReceivedDate { get; set; }
    }
}
