using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.Entities
{
    public class DealerDailyInventory
    {
        public Guid Id { get; set; }
        public Guid DealerId { get; set; }
        public Guid EVTemplateId { get; set; }

        [Column(TypeName = "date")]
        public DateTime SnapshotDate { get; set; }
        public int OpeningStock { get; set; }
        public int Inflow { get; set; }
        public int Outflow { get; set; }
        public int ClosingStock { get; set; }
        public string? Note { get; set; }
        public Dealer Dealer { get; set; } = null!;
        public ElectricVehicleTemplate EVTemplate { get; set; } = null!;
    }
}
