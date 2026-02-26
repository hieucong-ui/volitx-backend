using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.Customer
{
    public class UpdateCustomerDTO
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? CitizenID { get; set; } //CCCD 
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? Note { get; set; }
    }
}
