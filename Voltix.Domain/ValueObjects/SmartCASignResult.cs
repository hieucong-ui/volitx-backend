using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.ValueObjects
{
    public record SmartCASignResult
    {
        public string TransactionId { get; set; } = null!;
        public string CmsSignatureBase64 { get; set; } = null!;
        public string CertSerial { get; set; } = null!;
        public string CertPem { get; set; } = null!;
        public DateTime SignedAtUtc { get; set; }
    }
}
