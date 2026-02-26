using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.EContract
{
    public class GetEContractDTO
    {
        public Guid Id { get; private set; }
        public string HtmlTemaple { get; private set; } = null!;
        public string? Name { get; private set; }
        public EContractStatus Status { get; private set; }
        public EcontractType Type { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public string CreatedBy { get; private set; } = null!;
        public string? CreatedName { get; set; }
        public string OwnerBy { get; private set; } = null!;
        public string? OwnerName { get; private set; }
        public Guid? CustomerOrderId { get; private set; }
    }
}
