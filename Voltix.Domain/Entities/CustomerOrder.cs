using Voltix.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Voltix.Domain.Entities
{
    public class CustomerOrder
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid QuoteId { get; set; }
        public int OrderNo { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal? DepositAmount { get; set; }
        public OrderStatus Status { get; set; }

        public Customer Customer { get; set; } = null!;
        public Quote Quote { get; set; } = null!;
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ApplicationUser? CreatedByUser { get; set; }
        public ICollection<EContract>? EContracts { get; set; } = new List<EContract>();
    }
}
