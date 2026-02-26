using Voltix.Application.DTO.OrderDetail;
using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.CustomerOrder
{
    public class CreateCustomerOrderDTO
    {
        public Guid CustomerId { get; set; }
        public Guid QuoteId { get; set; }
        public bool IsPayFull { get; set; }
    }
}