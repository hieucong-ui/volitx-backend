using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.Dealer
{
    public class ForecastTargetDTO
    {
        public Guid DealerId { get; set; }
        public Guid EVTemplateId { get; set; }
        public int OpeningStock { get; set; }
        public int ClosingStock { get; set; }
        public DateTime SnapshotDate { get; set; }
    }
}
