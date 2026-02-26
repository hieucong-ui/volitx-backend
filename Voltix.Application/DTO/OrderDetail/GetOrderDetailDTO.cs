using Voltix.Application.DTO.ElectricVehicle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.OrderDetail
{
    public class GetOrderDetailDTO
    {
        public Guid Id { get; set; }
        public Guid CustomerOrderId { get; set; }
        public Guid ElectricVehicleId { get; set; }
        public GetElecticVehicleDTO? ElectricVehicle { get; set; }
    }
}
