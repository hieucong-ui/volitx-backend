using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Voltix.Domain.ValueObjects
{
    public class DeleteEContractDraftResponse
    {
        [JsonPropertyName("status")]
        public EnumInfoDto? Status { get; set; }
    }

    public class EnumInfoDto
    {
        [JsonPropertyName("value")]
        public int Value { get; set; }
    }
}
