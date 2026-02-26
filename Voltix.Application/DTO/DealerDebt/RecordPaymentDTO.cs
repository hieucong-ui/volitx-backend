using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.DealerDebt
{
    public class RecordPaymentDTO
    {
        public string? ReferenceNo { get; set; }
        public DateTime PaidAtUtc { get; set; }
        public decimal Amount { get; set; }

        public string? Method { get; set; }   
        public string? SourceType { get; set; }
        public Guid? SourceId { get; set; }
        public string? Note { get; set; }
    }
}
