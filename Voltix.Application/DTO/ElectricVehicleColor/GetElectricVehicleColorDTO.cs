using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.ElectricVehicleColor
{
    public class GetElectricVehicleColorDTO
    {
        public Guid Id { get; set; }
        public string ColorName { get; set; }
        public string ColorCode { get; set; }
        public decimal ExtraCost { get; set; }
    }
}
