using Voltix.Application.DTO.QuoteDetail;
using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.Quote
{
    public class CreateQuoteDTO
    {
        public string? Note { get; set; }
        public List<CreateQuoteDetailDTO> QuoteDetails { get; set; } = new();
    }
}
