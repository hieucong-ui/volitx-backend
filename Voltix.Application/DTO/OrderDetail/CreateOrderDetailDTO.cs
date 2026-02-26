using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.OrderDetail
{
    public class CreateOrderDetailDTO
    {
        public Guid CustomerOrderId { get; set; }
        public Guid ElectricVehicleId { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal? Discount { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
