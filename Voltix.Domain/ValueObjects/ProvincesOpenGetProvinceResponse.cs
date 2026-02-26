using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.ValueObjects
{
    public class ProvincesOpenGetProvinceResponse
    {
        [JsonProperty] public int Code { get; set; }
        [JsonProperty] public string Name { get; set; } = string.Empty;
        [JsonProperty] public string Division_type { get; set; } = string.Empty;
        [JsonProperty] public string Codename { get; set; } = string.Empty;
        [JsonProperty] public int Phone_code { get; set; }
    }
}
