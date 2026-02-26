using Voltix.Application.DTO.ElectricVehicle;
using Voltix.Application.DTO.EVTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.BookingEV
{
    public class GetVehicleByBookingDTO
    {
        public Guid ElectricVehicleId { get; set; }
        public string VIN { get; set; }
        public ViewVersionName? Version { get; set; }
        public ViewColorName? Color { get; set; }
        public ViewWarehouse? Warehouse { get; set; }
    }
}
