using Voltix.Application.DTO.BookingEVDetail;
using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.BookingEV
{
    public class CreateBookingEVDTO
    {
        public string? Note { get; set; }
        public List<CreateBookingEVDetailDTO> BookingDetails { get; set; } = new();
    }
}
