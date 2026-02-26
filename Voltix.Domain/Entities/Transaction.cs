using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.Entities
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public Guid? CustomerOrderId { get; set; }
        public string Provider { get; set; } = null!;
        public string OrderRef { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;
        public TransactionStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? Note { get; set; }
        public CustomerOrder CustomerOrder { get; set; } = null!;
    }
}
