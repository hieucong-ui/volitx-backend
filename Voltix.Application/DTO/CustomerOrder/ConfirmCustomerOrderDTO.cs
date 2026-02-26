using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.CustomerOrder
{
    public class ConfirmCustomerOrderDTO
    {
        public Guid CustomerOrderId { get; set; }
        public bool IsPayFull { get; set; }
        public bool IsCash { get; set; }
    }
}
