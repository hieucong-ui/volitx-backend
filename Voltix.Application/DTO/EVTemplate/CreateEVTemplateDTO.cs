using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.EVTemplate
{
    public class CreateEVTemplateDTO
    {
        public Guid VersionId { get; set; }
        public Guid ColorId { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public List<string>? AttachmentKeys { get; set; } = new();
        public bool IsActive { get; set; } = true;
    }
}
