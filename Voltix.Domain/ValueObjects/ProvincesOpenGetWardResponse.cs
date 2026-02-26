using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.ValueObjects
{
    public class ProvincesOpenGetWardResponse
    {
        [JsonProperty] public int Code { get; set; }
        [JsonProperty] public string Name { get; set; } = string.Empty;
        [JsonProperty] public string Division_type { get; set; } = string.Empty;
        [JsonProperty] public string Codename { get; set; } = string.Empty;
        [JsonProperty] public int Phone_code { get; set; }
        [JsonProperty] public List<Wards> Wards { get; set; } = new List<Wards>();
    }

    public class Wards
    {
        [JsonProperty] public int Code { get; set; }
        [JsonProperty] public string Name { get; set; }
        [JsonProperty] public string Division_type { get; set; }
        [JsonProperty] public string Codename { get; set; }
        [JsonProperty] public int Province_code { get; set; }
    }
}
