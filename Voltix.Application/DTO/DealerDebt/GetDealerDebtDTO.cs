using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.DealerDebt
{
    public class GetDealerDebtDTO
    {
        public Guid Id { get; set; }
        public Guid DealerId { get; set; }
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal PurchasesAmount { get; set; }
        public decimal PaymentsAmount { get; set; }
        public decimal CommissionsAmount { get; set; }
        public decimal PenaltiesAmount { get; set; }
        public decimal ClosingBalance { get; set; }
        public decimal OverpaidAmount { get; set; }
        public string? ReferenceNo { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
    }
}
