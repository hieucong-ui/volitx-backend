using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.ValueObjects
{
    public record VnptCreateDocReq(string No, string Subject, int TypeId, int DepartmentId, string? Description = null);
}
