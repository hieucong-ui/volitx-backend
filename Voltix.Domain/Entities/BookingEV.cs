using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.Entities
{
    public class BookingEV
    {
        public Guid Id { get; set; }
        public Guid DealerId { get; set; }
        public Guid? EContractId { get; set; }
        public DateTime BookingDate { get; set; }
        public BookingStatus Status { get; set; }
        public int TotalQuantity { get; set; }
        public string? Note { get; set; }
        public string? CreatedBy { get; set; }

        public ICollection<BookingEVDetail> BookingEVDetails { get; set; } = new List<BookingEVDetail>();
        public Dealer Dealer { get; set; } = null!;
        public VehicleDelivery VehicleDelivery { get; set; } = null!;
        public EContract? EContract { get; set; }
    }
}
