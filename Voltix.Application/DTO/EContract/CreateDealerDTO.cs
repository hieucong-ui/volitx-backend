using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.EContract
{
    public class CreateDealerDTO
    {
        public string DealerName { get; set; } = null!;
        public string DealerAddress { get; set; } = null!;
        public string TaxNo { get; set; } = null!;
        public string BankAccount { get; set; } = null!;
        public string BankName { get; set; } = null!;
        public int DealerLevel { get; set; }
        public string FullNameManager { get; set; } = null!;
        public string EmailManager { get; set; } = null!;
        public string? PhoneNumberManager { get; set; }
    }
}
