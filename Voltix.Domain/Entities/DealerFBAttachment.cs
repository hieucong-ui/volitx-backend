using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.Entities
{
    public class DealerFBAttachment
    {
        public Guid Id { get; set; }
        public string Key { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public Guid DealerFeedBackId { get; set; }

        public DealerFeedback DealerFeedback { get; set; } = null!;
    }
}
