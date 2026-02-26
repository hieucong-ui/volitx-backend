using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.Entities
{
    public class ElectricVehicleModel
    {
        public Guid Id { get; set; }
        public string? ModelName { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public StatusModel Status { get; set; } = StatusModel.Available;

        public ICollection<ElectricVehicleVersion> Versions { get; set; } = new List<ElectricVehicleVersion>();
        public Promotion? Promotion { get; set; }
    }
}
