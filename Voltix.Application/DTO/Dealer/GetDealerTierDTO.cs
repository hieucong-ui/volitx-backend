using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.Dealer
{
    public class GetDealerTierDTO
    {
        public Guid Id { get; set; }
        public int Level { get; set; }
        public string? Name { get; set; }
        public decimal? BaseCommissionPercent { get; set; }
        public decimal? BaseCreditLimit { get; set; }
        public decimal? BaseLatePenaltyPercent { get; set; }
        public decimal? BaseDepositPercent { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
