using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.Entities
{
    public class DealerInventoryForecast
    {
        public Guid Id { get; set; }
        public Guid DealerId { get; set; }
        public Guid EVTemplateId { get; set; }
        public DateTime TargetDate { get; set; } 
        public double Forecast { get; set; }   
        public double? ForecastLower { get; set; } 
        public double? ForecastUpper { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public string? ModelVersion { get; set; }

        public Dealer Dealer { get; set; } = null!;
        public ElectricVehicleTemplate EVTemplate { get; set; } = null!;
    }
}
