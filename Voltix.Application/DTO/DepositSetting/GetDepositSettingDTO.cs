using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.DepositSetting
{
    public class GetDepositSettingDTO
    {
        public Guid Id { get; set; }
        public string ManagerId { get; set; } = null!;
        public string ManagerName { get; set; } = null!;
        public Guid? DealerId { get; set; }
        public string? DealerName { get; set; }
        public decimal? MinDepositPercentage { get; set; }
        public decimal MaxDepositPercentage { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
