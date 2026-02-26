using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.Entities
{
    public class Warehouse
    {
        public Guid Id { get; set; }
        public Guid? DealerId { get; set; }
        public Guid? EVCInventoryId { get; set; }
        public int AlertNumber { get; set; } = 10;
        public string? WarehouseName { get; set; }
        public WarehouseType WarehouseType { get; set; }

        public ICollection<ElectricVehicle> ElectricVehicles { get; set; } = new List<ElectricVehicle>();
        public Dealer? Dealer { get; set; }
        public EVCInventory? EVCInventory { get; set; }
    }
}
