using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.Entities
{
    public class DealerFeedback
    {
        public Guid Id { get; set; }
        public Guid DealerId { get; set; }
        public string? FeedbackContent { get; set; }
        public FeedbackStatus Status { get; set; } = FeedbackStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Dealer Dealer { get; set; } = null!;
        public ICollection<DealerFBAttachment> DealerFBAttachments { get; set; } = new List<DealerFBAttachment>();
    }
}
