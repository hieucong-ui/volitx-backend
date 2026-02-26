using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.DealerConfiguration
{
    public class UpsertDealerConfigurationDTO
    {
        public bool? AllowOverlappingAppointments { get; set; }
        public int? MaxConcurrentAppointments { get; set; }
        public TimeSpan? OpenTime { get; set; }
        public TimeSpan? CloseTime { get; set; }
        public int? MinIntervalBetweenAppointments { get; set; }
        public int? BreakTimeBetweenAppointments { get; set; }
        public decimal? MinDepositPercentage { get; set; }
        public decimal? MaxDepositPercentage { get; set; }
    }
}
