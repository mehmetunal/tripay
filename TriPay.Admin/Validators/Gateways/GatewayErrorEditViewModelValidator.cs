using FluentValidation;
using TriPay.Admin.Models.Gateways;

namespace TriPay.Admin.Validators.Gateways;

public sealed class GatewayErrorEditViewModelValidator : AbstractValidator<GatewayErrorEditViewModel>
{
    private static readonly string[] AllowedLocales = ["tr", "en"];

    public GatewayErrorEditViewModelValidator()
    {
        RuleFor(x => x.PaymentGatewayId).GreaterThan(0);
        RuleFor(x => x.ProviderErrorCode)
            .NotEmpty().WithMessage("Provider hata kodu zorunludur.")
            .MaximumLength(64);
        RuleFor(x => x.NormalizedCode).MaximumLength(32).When(x => !string.IsNullOrWhiteSpace(x.NormalizedCode));
        RuleFor(x => x.UserMessage)
            .NotEmpty().WithMessage("Kullanıcı mesajı zorunludur.")
            .MaximumLength(512);
        RuleFor(x => x.Locale)
            .NotEmpty().WithMessage("Dil zorunludur.")
            .MaximumLength(8)
            .Must(l => AllowedLocales.Contains(l, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Dil tr veya en olmalıdır.");
    }
}
