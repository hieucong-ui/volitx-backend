using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.ValueObjects
{
    public class VnptUserDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Name { get; set; } = null!;
    }
}
