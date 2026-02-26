using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.Dealer
{
    public class DealerEffectivePolicyDTO
    {
        public Guid DealerId { get; set; }
        public string DealerName { get; set; } = string.Empty;
        public Guid? DealerTierId { get; set; }
        public int? DealerTierLevel { get; set; }
        public string? DealerTierName { get; set; }
        public string Source { get; set; } = "Default";
        public decimal? CommissionPercent { get; set; }
        public decimal? CreditLimit { get; set; }
        public decimal? LatePenaltyPercent { get; set; }
        public decimal? DepositPercent { get; set; }
        public Guid? OverrideId { get; set; }
        public string? OverrideNote { get; set; }
        public DateTime? OverrideEffectiveFrom { get; set; }
        public DateTime? OverrideEffectiveTo { get; set; }
        public DateTime ResolvedAt { get; set; } = DateTime.UtcNow;
    }
}
