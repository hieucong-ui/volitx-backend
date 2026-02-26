using Voltix.Application.DTO.ElectricVehicleVersion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.BookingEVDetail
{
    public class GetBookingEVDetailDTO
    {
        public Guid Id { get; set; }
        public Guid BookingId { get; set; }
        public VersionDTO Version { get; set; }
        public Guid ColorId { get; set; }
        public int Quantity { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        
    }
    public class VersionDTO
    {
        public Guid VersionId { get; set; }
        public Guid ModelId { get; set; }
    }
}
