using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.ValueObjects
{
    public class GhnResult<T>
    {
        [JsonProperty("code")] public int Code { get; set; }
        [JsonProperty("message")] public string? Message { get; set; }
        [JsonProperty("data")] public T? Data { get; set; }
        public bool Success => Code == 200;
        public static GhnResult<T> Fail(string msg) => new() { Code = -1, Message = msg, Data = default };
    }
}
