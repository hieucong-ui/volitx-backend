using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.ElectricVehicleVersion
{
    public class GetElectricVehicleVersionDTO
    {
        public Guid? Id { get; set; }
        public Guid ModelId { get; set; }
        public string VersionName { get; set; }
        public decimal MotorPower { get; set; }
        public decimal BatteryCapacity { get; set; }
        public int RangePerCharge { get; set; }
        public int TopSpeed { get; set; }
        public decimal Weight { get; set; }
        public decimal Height { get; set; }
        public int ProductionYear { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

    
