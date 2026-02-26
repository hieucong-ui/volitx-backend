using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO
{
    public class FileInfoDTO
    {
        public string? FilePath { get; set; }
        public byte[]? File { get; set; }
        public string? FileName { get; set; }
    }
}
