using System.Linq.Expressions;
using FluentValidation;

namespace TriPay.Admin.Validators;

/// <summary>Panel formları için ortak FluentValidation kuralları.</summary>
internal static class AdminValidationRules
{
    public const int PasswordMinLength = 8;
    public const int PasswordMaxLength = 100;
    public const int DisplayNameMaxLength = 128;
    public const int MerchantNameMaxLength = 256;
    public const int WebhookUrlMaxLength = 512;

    public static IRuleBuilderOptions<T, string> AdminEmail<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage("E-posta zorunludur.")
            .EmailAddress().WithMessage("Geçerli bir e-posta girin.")
            .MaximumLength(256);

    public static IRuleBuilderOptions<T, string> AdminPassword<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage("Şifre zorunludur.")
            .MinimumLength(PasswordMinLength).WithMessage("Şifre en az 8 karakter olmalıdır.")
            .MaximumLength(PasswordMaxLength);

    public static IRuleBuilderOptions<T, string> AdminPasswordConfirm<T>(
        this IRuleBuilder<T, string> rule,
        Expression<Func<T, string>> passwordExpression) =>
        rule.NotEmpty().WithMessage("Şifre tekrarı zorunludur.")
            .Equal(passwordExpression).WithMessage("Şifreler eşleşmiyor.");
}
