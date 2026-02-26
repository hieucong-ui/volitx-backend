using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.BookingEVDetail
{
    public class CreateBookingEVDetailDTO
    {
        public Guid VersionId { get; set; }
        public Guid ColorId { get; set; }
        public int Quantity { get; set; }
    }
}
