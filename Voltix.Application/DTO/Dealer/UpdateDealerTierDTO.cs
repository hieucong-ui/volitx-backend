using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.Dealer
{
    public class UpdateDealerTierDTO
    {
        public string? Name { get; set; }
        public int? Level { get; set; }
        public decimal? BaseCommissionPercent { get; set; }
        public decimal? BaseDepositPercent { get; set; }
        public decimal? BaseLatePenaltyPercent { get; set; }
        public decimal? BaseCreditLimit { get; set; }
        public string? Description { get; set; }
    }
}
