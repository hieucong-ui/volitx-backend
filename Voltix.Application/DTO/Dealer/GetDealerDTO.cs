using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.Dealer
{
    public class GetDealerDTO
    {
        public Guid Id { get; set; }
        public string? ManagerId { get; set; }
        public string? ManagerName { get; set; }
        public string? ManagerEmail { get; set; }
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string TaxNo { get; set; } = null!;
        public int Level { get; set; }
        public Guid? DealerTierId { get; set; }
        public string? BankAccount { get; set; }
        public string? BankName { get; set; }
        public DealerStatus DealerStatus { get; set; } = DealerStatus.Inactive;
    }
}
