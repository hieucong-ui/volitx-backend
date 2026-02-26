using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.Entities
{
    public class DealerInventoryRisk
    {
        public Guid Id { get; set; }
        public Guid DealerId { get; set; }
        public Guid EVTemplateId { get; set; }
        public DateTime TargetDate { get; set; }
        public int ExpectedClosing { get; set; }
        public InventoryRiskLevel RiskLevel { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsResolved { get; set; }

        public Dealer Dealer { get; set; } = null!;
        public ElectricVehicleTemplate EVTemplate { get; set; } = null!;
    }
}
