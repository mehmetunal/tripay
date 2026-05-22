using FluentValidation;
using TriPay.Admin.Models.Merchants;
using TriPay.Admin.Validators;

namespace TriPay.Admin.Validators.Merchants;

public sealed class MerchantEditViewModelValidator : AbstractValidator<MerchantEditViewModel>
{
    public MerchantEditViewModelValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ad zorunludur.")
            .MaximumLength(AdminValidationRules.MerchantNameMaxLength);
        RuleFor(x => x.WebhookUrl)
            .MaximumLength(AdminValidationRules.WebhookUrlMaxLength)
            .Must(BeValidUrlOrEmpty).WithMessage("Geçerli bir URL girin.")
            .When(x => !string.IsNullOrWhiteSpace(x.WebhookUrl));
    }

    private static bool BeValidUrlOrEmpty(string? url) =>
        string.IsNullOrWhiteSpace(url) || Uri.TryCreate(url.Trim(), UriKind.Absolute, out _);
}
