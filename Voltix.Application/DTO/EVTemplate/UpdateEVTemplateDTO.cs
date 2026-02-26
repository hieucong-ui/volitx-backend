using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.EVTemplate
{
    public class UpdateEVTemplateDTO
    {
        public decimal? Price { get; set; }
        public string? Description { get; set; }
        public List<string> AttachmentKeys { get; set; } = new();
    }
}
