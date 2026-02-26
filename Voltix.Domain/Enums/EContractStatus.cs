using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.Enums
{
    public enum EContractStatus
    {
        Draft = 1,
        Ready = 2,
        InProgress = 3,
        Completed = 4,
        Correcting = 5,
        Accepted = 6,
        Cancelled = -3,
        Deleted = -2,
        Rejected = -1
    }
}
