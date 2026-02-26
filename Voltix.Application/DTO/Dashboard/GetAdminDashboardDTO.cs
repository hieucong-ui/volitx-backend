using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.Dashboard
{
    public class GetAdminDashboardDTO
    {
        public int TotalEVCInventory { get; set; }
        public int TotalVehicles { get; set; }
        public int TotalDealers { get; set; }
        public int TotalBookings { get; set; }
        public int TotalDeliveries { get; set; }
        public int TotalEVMStaff { get; set; }
    }
}
