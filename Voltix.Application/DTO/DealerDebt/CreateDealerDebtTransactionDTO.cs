using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.DealerDebt
{
    public class CreateDealerDebtTransactionDTO
    {
        public Guid DealerId { get; set; }
        public DealerDebtTransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public bool? IsIncrease { get; set; }
        public DateTime OccurredAtUtc { get; set; }


        public string ExternalId { get; set; } = null!;
        public string? SourceType { get; set; }
        public Guid? SourceId { get; set; }
        public string? SourceNo { get; set; }
        public string? Method { get; set; }
        public string? ReferenceNo { get; set; }
        public string? Note { get; set; }
    }
}
