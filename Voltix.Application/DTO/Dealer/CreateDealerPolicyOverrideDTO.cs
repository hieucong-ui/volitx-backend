using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.Dealer
{
    public class CreateDealerPolicyOverrideDTO
    {
        public decimal? CommissionPercent { get; set; }
        public decimal? CreditLimit { get; set; }
        public decimal? LatePenaltyPercent { get; set; }
        public decimal? DepositPercent { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }

        public string? Note { get; set; }
    }
}
