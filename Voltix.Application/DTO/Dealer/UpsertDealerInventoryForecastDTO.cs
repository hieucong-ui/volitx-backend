using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.Dealer
{
    public class UpsertDealerInventoryForecastDTO
    {
        public Guid DealerId { get; set; }
        public Guid EVTemplateId { get; set; }
        public DateTime TargetDate { get; set; }
        public double Forecast { get; set; }
        public double? ForecastLower { get; set; }
        public double? ForecastUpper { get; set; }
        public string? ModelVersion { get; set; }
    }
}
