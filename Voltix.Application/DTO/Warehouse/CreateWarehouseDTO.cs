using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.Warehouse
{
    public class CreateWarehouseDTO
    {
        public Guid? DealerId { get; set; }
        public Guid? EVCInventoryId { get; set; }
        public string? WarehouseName { get; set; }
        public WarehouseType WarehouseType { get; set; }
    }
}
