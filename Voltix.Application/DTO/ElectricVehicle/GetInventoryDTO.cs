using Voltix.Application.DTO.EVTemplate;
using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.ElectricVehicle
{
    public class GetInventoryDTO
    {
        public ViewVersionName? Version { get; set; }
        public ViewColorName? Color { get; set; }
        public int TotalQuantity { get; set; }
        public DateTime? ImportDate { get; set; }
        public List<WarehouseQuantityDTO> Warehouses { get; set; } = new();
    }

    public class WarehouseQuantityDTO
    {
        public Guid WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
