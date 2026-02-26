using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.ValueObjects
{
    public record VnptStatusDto
    {
        public int Value { get; init; }
        public string? Description { get; init; }
    }

}
