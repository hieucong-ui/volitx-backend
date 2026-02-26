using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.ValueObjects
{
    public class AddNewSmartCADTO
    {
        public required int UserId { get; set; }
        public required string UserName { get; set; }
        public string? SerialNumber { get; set; }
    }
}
