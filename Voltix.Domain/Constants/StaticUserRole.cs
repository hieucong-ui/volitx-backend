using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.Constants
{
    public static class StaticUserRole
    {
        public const string Admin = "Admin";
        public const string DealerManager = "DealerManager";
        public const string DealerStaff = "DealerStaff";
        public const string EVMStaff = "EVMStaff";
        public const string Admin_EVMStaff = Admin + "," + EVMStaff;
        public const string DealerManager_DealerStaff = DealerManager + "," + DealerStaff;
        public const string AllRolesInSystem = Admin + "," + DealerManager + "," + DealerStaff + "," + EVMStaff;
    }
}
