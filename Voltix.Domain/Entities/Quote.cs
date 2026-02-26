using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.Entities
{
    public class Quote
    {
        public Guid Id { get; set; }
        public Guid DealerId { get; set; } 
        public string CreatedBy { get; set; } = null!; 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public QuoteStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Note { get; set; }
        

        public Dealer Dealer { get; set; } = null!;
        public ICollection<QuoteDetail> QuoteDetails { get; set; } = new List<QuoteDetail>();
        public ApplicationUser CreatedByUser { get; set; } = null!;
        public ICollection<CustomerOrder> CustomerOrders { get; set; } = new List<CustomerOrder>(); 
    }
}
