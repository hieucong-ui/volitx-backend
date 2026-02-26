using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.Enums
{
    public enum ElectricVehicleStatus
    {
        Available = 1,
        Pending = 2,
        Booked = 3,
        InTransit = 4,
        Sold = 5,
        AtDealer = 6,
        Maintenance = 7,
        DealerPending = 8,
        DepositBooked = 9,
        Broken = 10,
        NotWork = 11
    }
}
