using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.Appointment
{
    public class GetCreateAppointmentDTO
    {
        public Guid Id { get; set; }
        public Guid DealerId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid EVTemplateId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public AppointmentStatus Status { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
