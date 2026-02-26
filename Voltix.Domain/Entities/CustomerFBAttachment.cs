using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.Entities
{
    public class CustomerFBAttachment
    {
        public Guid Id { get; set; }
        public string Key { get; set; } = null!;
        public string? FileName { get; set; }
        public Guid CustomerFeedBackId { get; set; }

        public CustomerFeedback CustomerFeedback { get; set; } = null!;
    }
}
