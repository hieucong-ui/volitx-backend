using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.Entities
{
    public class ElectricVehicleTemplate
    {
        public Guid Id { get; set; }                  
        public Guid VersionId { get; set; }          
        public Guid ColorId { get; set; }             
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; } = true;

        public ElectricVehicleVersion Version { get; set; } = null!;
        public ElectricVehicleColor Color { get; set; } = null!;
        public ICollection<ElectricVehicle> ElectricVehicles { get; set; } = new List<ElectricVehicle>();
        public ICollection<EVAttachment> EVAttachments { get; set; } = new List<EVAttachment>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<DealerDailyInventory> DealerDailyInventories { get; set; } = new List<DealerDailyInventory>();
        public ICollection<DealerInventoryForecast> DealerInventoryForecasts { get; set; } = new List<DealerInventoryForecast>();
        public ICollection<DealerInventoryRisk> DealerInventoryRisks { get; set; } = new List<DealerInventoryRisk>();
    }
}
