using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.ValueObjects
{
    public class ProcessLoginInfoDto
    {

        public string? ProcessId { get; set; }
        public string? DownloadUrl { get; set; }
        public int? ProcessedByUserId { get; set; }
        public string? AccessToken { get; set; }
        public string? Position { get; set; }
        public int? PageSign { get; set; }
        public bool IsOTP { get; set; }
    }
}
