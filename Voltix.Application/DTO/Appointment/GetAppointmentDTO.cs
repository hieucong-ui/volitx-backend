using Voltix.Application.DTO.ElectricVehicle;
using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.Appointment
{
    public class GetAppointmentDTO
    {
        public Guid Id { get; set; }
        public ViewDealerName? Dealer { get; set; }
        public ViewCustomerName? Customer { get; set; }
        public ViewTemplate? EVTemplate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public AppointmentStatus Status { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    public class ViewDealerName
    {
        public Guid DealerId { get; set; }
        public string? DealerName { get; set; }
    }
    public class ViewCustomerName
    {
        public Guid CustomerId { get; set; }
        public string? CustomerName { get; set; }
    }
}
