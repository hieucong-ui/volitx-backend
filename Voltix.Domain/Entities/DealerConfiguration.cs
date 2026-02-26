using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.Entities
{
    public class DealerConfiguration
    {
        public Guid Id { get; set; }
        public string ManagerId { get; set; } = null!;
        public Guid? DealerId { get; set; }
        public bool AllowOverlappingAppointments { get; set; }
        public int MaxConcurrentAppointments { get; set; }
        public TimeSpan OpenTime { get; set; }
        public TimeSpan CloseTime { get; set; }
        public int MinIntervalBetweenAppointments { get; set; }
        public int BreakTimeBetweenAppointments { get; set; }
        public decimal? MinDepositPercentage { get; set; }
        public decimal MaxDepositPercentage { get; set; }
        public int DayCancelDeposit { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ApplicationUser Manager { get; set; } = null!;
        public Dealer? Dealer { get; set; }
    }
}
