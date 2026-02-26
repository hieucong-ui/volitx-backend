using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.Appointment
{
    public class CreateAppointmentDTO
    {
        public Guid CustomerId { get; set; }
        public Guid EVTemplateId { get; set; }
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public DateTime EndTime { get; set; } = DateTime.UtcNow;
        public AppointmentStatus Status { get; set; }
        public string? Note { get; set; }
       
    }
}
