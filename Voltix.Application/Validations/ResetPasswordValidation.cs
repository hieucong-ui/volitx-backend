using FluentValidation;
using Voltix.Application.DTO.Auth;

namespace Voltix.Application.Validations
{
    public class ResetPasswordValidation : AbstractValidator<ResetPasswordDTO>
    {
        public ResetPasswordValidation() 
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required.");

            RuleFor(x => x.Token).NotEmpty().WithMessage("Token is required.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must contain at least 1 special character, 1 uppercase latter, 1 number and at least 8 characters");

            RuleFor(x => x.ConfirmPassword).NotEmpty().WithMessage("Confirm Password is required.")
                .Equal(x => x.Password).WithMessage("Password do not match");
        }
    }
}
