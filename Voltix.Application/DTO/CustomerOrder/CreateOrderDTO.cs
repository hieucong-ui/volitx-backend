using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.CustomerOrder
{
    public class CreateOrderDTO
    {
        public Guid CustomerId { get; set; }
        public Guid QuoteId { get; set; }
    }
}
