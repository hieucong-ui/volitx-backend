using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.Auth
{
    public class ChangePasswordDTO
    {
        public string CurrentPassword { get; set; } = null!;

        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[!@#$%^&*(),.?""{}|<>])(?=.*\d).{8,}$")]
        public string NewPassword { get; set; } = null!;

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Password is not matching with confirm password")]
        public string ConfirmNewPassword { get; set; } = null!;
    }
}
