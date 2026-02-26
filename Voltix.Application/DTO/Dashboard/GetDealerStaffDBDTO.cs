using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.Dashboard
{
    public class GetDealerStaffDBDTO
    {
        public string? DealerName { get; set; }
        public int TotalQuotes { get; set; }
        public int TotalVehicles { get; set; }
        public int TotalCustomers { get; set; }
    }
}
