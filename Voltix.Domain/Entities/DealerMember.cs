using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.Entities
{
    public class DealerMember
    {
        public Guid DealerId { get; set; }
        public string ApplicationUserId { get; set; } = null!;
        public DealerRole RoleInDealer { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LeftAt { get; set; }

        public ApplicationUser ApplicationUser { get; set; } = null!;
        public Dealer Dealer { get; set; } = null!;

    }
}
