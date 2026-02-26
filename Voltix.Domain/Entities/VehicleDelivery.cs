using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.Entities
{
    public class VehicleDelivery
    {
        public Guid Id { get; set; }
        public Guid BookingEVId { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DeliveryStatus Status { get; set; }
        public DateTime? UpdateAt { get; set; } = DateTime.UtcNow;

        public BookingEV BookingEV { get; set; } = null!;
        public ICollection<VehicleDeliveryDetail> VehicleDeliveryDetails { get; set; } = new List<VehicleDeliveryDetail>();
    }
}
