using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.Dashboard
{
    public class GetDealerRevenueByQuarterDTO
    {
        public int Year { get; set; }
        public int Quarter { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalSoldVehicles { get; set; }
    }
}
