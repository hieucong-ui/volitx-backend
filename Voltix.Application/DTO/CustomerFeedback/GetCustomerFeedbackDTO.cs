using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.CustomerFeedback
{
    public class GetCustomerFeedbackDTO
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }
        public Guid DealerId { get; set; }
        public string? FeedbackContent { get; set; }
        public FeedbackStatus Status { get; set; } = FeedbackStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<string> ImgUrls { get; set; } = new();
    }
}
