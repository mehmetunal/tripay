using FluentValidation;
using TriPay.Admin.Models.Account;

namespace TriPay.Admin.Validators.Account;

public sealed class LoginViewModelValidator : AbstractValidator<LoginViewModel>
{
    public LoginViewModelValidator()
    {
        RuleFor(x => x.Email).AdminEmail();
        RuleFor(x => x.Password).NotEmpty().WithMessage("Şifre zorunludur.");
    }
}
