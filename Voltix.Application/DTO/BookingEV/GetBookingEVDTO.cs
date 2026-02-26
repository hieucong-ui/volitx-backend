using Voltix.Application.DTO.BookingEVDetail;
using Voltix.Application.DTO.EContract;
using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.BookingEV
{
    public class GetBookingEVDTO
    {
        public Guid Id { get; set; }
        public Guid DealerId { get; set; } // Đại lý thực hiện booking
        public DateTime BookingDate { get; set; }
        public BookingStatus Status { get; set; } // Enum trạng thái
        public int TotalQuantity { get; set; }
        public string? Note { get; set; }
        public string? CreatedBy { get; set; }
        public List<GetBookingEVDetailDTO> BookingEVDetails { get; set; } 
        public GetEContractDTO? EContract { get; set; }
    }
}
