using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.ValueObjects
{
    public record SmartCATransactionCreated
    {
        public string TransactionId { get; set; } = null!;
        public string? TransactionType { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
    }
}
