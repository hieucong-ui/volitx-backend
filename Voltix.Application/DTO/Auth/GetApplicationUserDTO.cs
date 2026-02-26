using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.Auth
{
    public class GetApplicationUserDTO
    {
        public string? Id { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public string? Sex { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public bool LockoutEnabled { get; set; }
    }
}
