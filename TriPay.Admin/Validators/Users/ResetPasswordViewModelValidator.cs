using FluentValidation;
using TriPay.Admin.Models.Users;
using TriPay.Admin.Validators;

namespace TriPay.Admin.Validators.Users;

public sealed class ResetPasswordViewModelValidator : AbstractValidator<ResetPasswordViewModel>
{
    public ResetPasswordViewModelValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0).WithMessage("Geçersiz kullanıcı.");
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Yeni şifre zorunludur.")
            .MinimumLength(AdminValidationRules.PasswordMinLength)
            .WithMessage("Şifre en az 8 karakter olmalıdır.")
            .MaximumLength(AdminValidationRules.PasswordMaxLength);
        RuleFor(x => x.ConfirmPassword).AdminPasswordConfirm(x => x.Password);
    }
}
