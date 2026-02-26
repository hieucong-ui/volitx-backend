using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.ElectricVehicleModel
{
    public class CreateElectricVehicleModelDTO
    {
        
        public string? ModelName { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public StatusModel Status { get; set; } = StatusModel.Available;
    }
}
