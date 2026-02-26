using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.Enums
{
    public enum BookingStatus
    {
        WaitingDealerSign = 1,
        Pending = 2,
        Approved = 3,
        Rejected = 4,
        Cancelled = 5,
        SignedByAdmin = 6,
        Completed = 7
    }
}
