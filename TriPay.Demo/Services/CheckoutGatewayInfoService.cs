using Microsoft.Extensions.Options;
using TriPay.Core.Gateways;
using TriPay.Core.Options;
using TriPay.Services;

namespace TriPay.Demo.Services;

/// <summary>Checkout ekranında gösterilecek ödeme kanalı bilgisi.</summary>
public sealed class CheckoutGatewayInfoService(IOptions<TriPayOptions> options)
{
    public CheckoutGatewaySnapshot GetSnapshot()
    {
        var triPay = options.Value;
        var activeCode = string.IsNullOrWhiteSpace(triPay.DefaultGateway)
            ? PaymentGatewayNames.VakifPays
            : triPay.DefaultGateway;

        var enabled = triPay.Gateways
            .Where(g => g.Value.Enabled)
            .Select(g => ToItem(g.Key, g.Value, g.Key.Equals(activeCode, StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(g => g.IsActive)
            .ThenBy(g => g.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        triPay.Gateways.TryGetValue(activeCode, out var activeConfig);
        var activeEnabled = activeConfig?.Enabled ?? false;

        return new CheckoutGatewaySnapshot(
            ActiveCode: activeCode,
            ActiveDisplayName: GetDisplayName(activeCode),
            IsTestMode: activeConfig?.IsTestMode ?? true,
            IsEnabled: activeEnabled,
            EnabledGateways: enabled);
    }

    private static EnabledGatewayItem ToItem(string code, GatewayConfig config, bool isActive) =>
        new(code, GetDisplayName(code), config.IsTestMode, isActive);

    public static string GetDisplayName(string code) => code switch
    {
        PaymentGatewayNames.VakifPays => "VakıfPayS",
        PaymentGatewayNames.Iyzico => "iyzico",
        PaymentGatewayNames.Vakifbank => "Vakıfbank Sanal POS",
        PaymentGatewayNames.Akbank => "Akbank Sanal POS",
        PaymentGatewayNames.Garanti => "Garanti BBVA Sanal POS",
        PaymentGatewayNames.IsBankasi => "İş Bankası Sanal POS",
        PaymentGatewayNames.YapiKredi => "Yapı Kredi Sanal POS",
        PaymentGatewayNames.Ziraat => "Ziraat Bankası Sanal POS",
        _ => code
    };
}

public sealed record CheckoutGatewaySnapshot(
    string ActiveCode,
    string ActiveDisplayName,
    bool IsTestMode,
    bool IsEnabled,
    IReadOnlyList<EnabledGatewayItem> EnabledGateways);

public sealed record EnabledGatewayItem(
    string Code,
    string DisplayName,
    bool IsTestMode,
    bool IsActive);
