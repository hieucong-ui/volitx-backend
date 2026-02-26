using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.ValueObjects
{
    public class GhnDistrict
    {
        public int DistrictID { get; set; }
        public int ProvinceID { get; set; }
        public string DistrictName { get; set; } = string.Empty;
        public string? Code { get; set; }
        public List<string>? NameExtension { get; set; }
    }
}
