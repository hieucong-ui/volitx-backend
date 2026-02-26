using Voltix.Application.DTO.QuoteDetail;
using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.Quote
{
    public class GetQuoteDTO
    {
        public Guid Id { get; set; }
        public Guid DealerId { get; set; }
        public string CreatedBy { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public QuoteStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Note { get; set; }
        public List<GetQuoteDetailDTO> QuoteDetails { get; set; } = new List<GetQuoteDetailDTO>();
    }
}
