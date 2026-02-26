using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.ValueObjects
{
    public class UpdateSmartDTO
    {
        public string Id { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public bool? IsSetDefault { get; set; }
        public string? Name { get; set; }
    }
}
