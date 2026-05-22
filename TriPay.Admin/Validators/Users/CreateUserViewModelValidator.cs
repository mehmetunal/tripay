using FluentValidation;
using TriPay.Admin.Models.Users;
using TriPay.Admin.Validators;
using TriPay.Data.Identity;

namespace TriPay.Admin.Validators.Users;

public sealed class CreateUserViewModelValidator : AbstractValidator<CreateUserViewModel>
{
    public CreateUserViewModelValidator()
    {
        RuleFor(x => x.Email).AdminEmail();
        RuleFor(x => x.DisplayName)
            .MaximumLength(AdminValidationRules.DisplayNameMaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.DisplayName));
        RuleFor(x => x.Password).AdminPassword();
        RuleFor(x => x.ConfirmPassword).AdminPasswordConfirm(x => x.Password);
        RuleFor(x => x.RoleName)
            .NotEmpty().WithMessage("Rol seçiniz.")
            .Must(AdminRoleExtensions.IsDefinedRoleName)
            .WithMessage("Geçersiz rol.");
    }
}
