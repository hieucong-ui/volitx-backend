using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.ValueObjects
{
    public class GhnWard
    {
        public string? WardCode { get; set; }
        public int DistrictID { get; set; }
        public string WardName { get; set; } = string.Empty;
        public List<string>? NameExtension { get; set; }
    }
}
