using FluentValidation;
using TriPay.Admin.Models.Gateways;

namespace TriPay.Admin.Validators.Gateways;

public sealed class GatewaySettingEditViewModelValidator : AbstractValidator<GatewaySettingEditViewModel>
{
    private static readonly string[] AllowedEnvironments = ["All", "Test", "Production"];

    public GatewaySettingEditViewModelValidator()
    {
        RuleFor(x => x.PaymentGatewayId).GreaterThan(0);
        RuleFor(x => x.SettingKey)
            .NotEmpty().WithMessage("Anahtar zorunludur.")
            .MaximumLength(128);
        RuleFor(x => x.SettingValue)
            .NotEmpty().WithMessage("Değer zorunludur.")
            .MaximumLength(1024);
        RuleFor(x => x.Environment)
            .NotEmpty().WithMessage("Ortam zorunludur.")
            .Must(env => AllowedEnvironments.Contains(env, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Ortam All, Test veya Production olmalıdır.");
    }
}
