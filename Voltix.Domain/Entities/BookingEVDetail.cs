using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.Entities
{
    public class BookingEVDetail
    {
        public Guid Id { get; set; }
        public Guid BookingId { get; set; }
        public Guid VersionId { get; set; }
        public Guid ColorId { get; set; }
        public int Quantity { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }

        public BookingEV BookingEV { get; set; } = null!;
        public ElectricVehicleVersion Version { get; set; } = null!;
        public ElectricVehicleColor Color { get; set; } = null!;

    }
}
