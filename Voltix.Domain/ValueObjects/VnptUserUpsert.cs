using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.ValueObjects
{
    public record VnptUserUpsert
    {
        public string Code { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public int ReceiveOtpMethod {  get; set; }
        public int ReceiveNotificationMethod { get; set; }
        public int SignMethod { get; set; }
        public bool SignConfirmationEnabled { get; set; }
        public bool GenerateSelfSignedCertEnabled { get; set; }
        public int Status { get; set; }
        public int receiveInfoAccountMethod { get; set; } = -1;
        public List<int>? DepartmentIds { get; set; } 
        public List<Guid>? RoleIds { get; set; }
    };
}
