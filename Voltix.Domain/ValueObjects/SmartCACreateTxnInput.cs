using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.ValueObjects
{
    public record SmartCACreateTxnInput
    {
        public string DocumentName { get; set; } = null!;
        public string DocumentHashBase64 { get; set; } = null!;
        public string SignerIdentifier { get; set; } = null!;
        public string reason { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string CallBackUrl { get; set; } = null!;
        public string? ReturnDeeplinkScheme { get; set; } = null!;
    }
}
