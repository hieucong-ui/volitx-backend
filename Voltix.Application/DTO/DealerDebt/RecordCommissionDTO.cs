using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.DealerDebt
{
    public class RecordCommissionDTO
    {
        public string? ReferenceNo { get; set; }
        public DateTime AtUtc { get; set; }
        public decimal Amount { get; set; }

        public string? SourceType { get; set; }
        public Guid? SourceId { get; set; }
        public string? Note { get; set; }
    }
}
