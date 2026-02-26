using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.Entities
{
    public class Notification
    {
        public Guid Id { get; set; }
        public Guid? DealerId { get; set; }
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string TargetRole { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;

        public Dealer? Dealer { get; set; }
    }

}
