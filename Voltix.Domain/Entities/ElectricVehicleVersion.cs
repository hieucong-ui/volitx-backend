using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.Entities
{
    public class ElectricVehicleVersion
    {
        public Guid Id { get; set; }
        public Guid ModelId { get; set; }
        public string? VersionName { get; set; }
        public decimal MotorPower { get; set; }
        public decimal BatteryCapacity { get; set; }
        public decimal RangePerCharge { get; set; }
        public decimal TopSpeed { get; set; }
        public decimal Weight { get; set; }
        public decimal Height { get; set; }
        public int ProductionYear { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        public ElectricVehicleModel Model { get; set; } = null!;
        public ICollection<ElectricVehicleTemplate> ElectricVehicleTemplates { get; set; } = new List<ElectricVehicleTemplate>();
        public ICollection<BookingEVDetail> BookingEVDetails { get; set; } = new List<BookingEVDetail>();
        public Promotion? Promotion { get; set; }
        public ICollection<QuoteDetail> QuoteDetails { get; set; } = new List<QuoteDetail>();
    }
}
