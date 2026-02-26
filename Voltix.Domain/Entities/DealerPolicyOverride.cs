using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.Entities
{
    public class DealerPolicyOverride
    {
        public Guid Id { get; set; }

        public Guid DealerId { get; set; }
        public decimal? CommissionPercent { get; set; }
        public decimal? CreditLimit { get; set; }
        public decimal? LatePenaltyPercent { get; set; }
        public decimal? DepositPercent { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        public Dealer Dealer { get; set; } = null!;
    }
}
