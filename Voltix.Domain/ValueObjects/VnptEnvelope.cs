using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.ValueObjects
{
    public record VnptEnvelope<T>
    {
        public T Data { set; get; }
        public bool Success { set; get; }
        public int Code { set; get; }
        public string[] Messages { set; get; }
    }
}
