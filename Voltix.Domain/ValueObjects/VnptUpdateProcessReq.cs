using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Voltix.Domain.ValueObjects
{
    public class VnptUpdateProcessDTO
    {
        public string? Id { get; set; }
        public bool ProcessInOrder { get; set; }
        public List<ProcessesRequestDTO>? Processes { get; set; }
    }
}
