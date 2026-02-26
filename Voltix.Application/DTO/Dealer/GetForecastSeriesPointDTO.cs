using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.Dealer
{
    public class GetForecastSeriesPointDTO
    {
        public DateTime TargetDate { get; set; }
        public double Forecast { get; set; }
        public double? ForecastLower { get; set; }
        public double? ForecastUpper { get; set; }
    }
}
