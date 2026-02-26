using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVManagementSystem.Application.DTO.EContract
{
    public class CreateEContractDTO
    {
        public Guid EContractId { get; set; }
        public string positionA { get; set; } = null!;
        public string positionB { get; set; } = null!;
        public int pageSign { get; set; }
    }
}
