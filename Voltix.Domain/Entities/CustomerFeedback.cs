using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.Entities
{
    public class CustomerFeedback
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid DealerId { get; set; }
        public string? FeedbackContent { get; set; }
        public FeedbackStatus Status { get; set; } = FeedbackStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Customer Customer { get; set; } = null!;
        public Dealer Dealer { get; set; } = null!;
        public ICollection<CustomerFBAttachment> CustomerFBAttachments { get; set; } = new List<CustomerFBAttachment>();
    }
}
