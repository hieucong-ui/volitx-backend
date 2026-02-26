using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.QuoteDetail
{
    public class CreateQuoteDetailDTO
    {
        public Guid VersionId { get; set; }
        public Guid ColorId { get; set; }
        public Guid? PromotionId { get; set; }
        public int Quantity { get; set; }
    }
}
