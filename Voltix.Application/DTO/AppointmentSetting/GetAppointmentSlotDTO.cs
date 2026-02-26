using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.AppointmentSetting
{
    public class GetAppointmentSlotDTO
    {
        public TimeSpan OpenTime { get; set; }
        public TimeSpan CloseTime { get; set; }
        public bool IsAvailable { get; set; }
    }
}
