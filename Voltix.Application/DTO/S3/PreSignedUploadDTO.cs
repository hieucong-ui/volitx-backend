using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.S3
{
    public class PreSignedUploadDTO
    {
        public string FileName { get; set; } = null!;
        public string ContentType { get; set; } = null!;
    }
}
