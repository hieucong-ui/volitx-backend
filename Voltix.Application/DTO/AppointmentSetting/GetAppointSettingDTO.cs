using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.AppointmentSetting
{
    internal class GetAppointSettingDTO
    {
        public Guid Id { get; set; }
        public string? ManagerId { get; set; }   
        public Guid? DealerId { get; set; } 
        public bool AllowOverlappingAppointments { get; set; }
        public int MaxConcurrentAppointments { get; set; }
        public TimeSpan OpenTime { get; set; }
        public TimeSpan CloseTime { get; set; }
        public int MinIntervalBetweenAppointments { get; set; }
        public int BreakTimeBetweenAppointments { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
