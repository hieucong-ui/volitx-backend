using Voltix.Application.DTO.VehicleDeliveryDetail;
using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.VehicleDelivery
{
    public class GetVehicleDeliveryDTO
    {
        public Guid Id { get; set; }
        public Guid BookingEVId { get; set; }
        public DeliveryStatus Status { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdateAt { get; set; }
        public List<GetVehicleDeliveryDetailDTO> VehicleDeliveryDetails { get; set; } = new List<GetVehicleDeliveryDetailDTO>();

    }
}
