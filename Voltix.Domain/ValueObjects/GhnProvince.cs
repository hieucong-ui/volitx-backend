using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.ValueObjects
{
    public class GhnProvince
    {
        public int ProvinceID { get; set; }
        public string ProvinceName { get; set; } = string.Empty;
        public string? Code { get; set; }
        public List<string>? NameExtension { get; set; }
    }
}
