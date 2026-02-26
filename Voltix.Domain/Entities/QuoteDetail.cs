using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Quic;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.Entities
{
    public class QuoteDetail
    {
        public Guid Id { get; set; }
        public Guid QuoteId { get; set; }
        public Guid VersionId { get; set; }
        public Guid ColorId { get; set; }
        public Guid? PromotionId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }

        public Promotion? Promotion { get; set; }
        public Quote Quote { get; set; } = null!;
        public ElectricVehicleVersion ElectricVehicleVersion { get; set; } = null!;
        public ElectricVehicleColor ElectricVehicleColor { get; set; } = null!;
        

    }
}
