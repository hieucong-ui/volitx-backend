using FluentValidation;
using Voltix.Application.DTO.Auth;

namespace Voltix.Application.Validations
{
    public class ForgotPasswordValidation : AbstractValidator<ForgotPasswordDTO>
    {
        public ForgotPasswordValidation()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");
        }
    }
}
