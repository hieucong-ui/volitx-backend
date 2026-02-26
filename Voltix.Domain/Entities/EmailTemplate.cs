using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.Entities
{
    public class EmailTemplate
    {
        public Guid Id { get; set; }
        public string TemplateName { get; set; } = default!;
        public string SubjectLine { get; set; } = default!;
        public string BodyContent { get; set; } = default!;
        public string SenderName { get; set; } = default!;
        public string Category { get; set; } = default!;
        public string? PreHeaderText { get; set; }
        public string? PersonalizationTags { get; set; }
        public string? FooterContent { get; set; }
        public string? CallToAction { get; set; }
        public string Language { get; set; } = default!;
        public string RecipientType { get; set; } = default!;
        public EmailStatus Status { get; set; } = EmailStatus.Active;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = default!;
        public string UpdatedBy { get; set; } = default!;
    }
}
