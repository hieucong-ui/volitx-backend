using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.Enums
{
    public enum DeliveryVehicleStatus
    {
        Preparing = 1,
        InTransit = 2,
        Delivered = 3,
        Damaged = 4,
        Replaced = 5
    }
}
