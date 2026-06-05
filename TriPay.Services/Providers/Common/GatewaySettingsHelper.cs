using TriPay.Core.Gateways;

namespace TriPay.Services.Providers.Common;

/// <summary>Gateway yapılandırma sözlüğünden ayar okuma yardımcıları.</summary>
public static class GatewaySettingsHelper
{
    /// <summary>İlk dolu ayar değerini döndürür.</summary>
    public static string? GetFirst(GatewayConfig config, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (config.Settings.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
                return value;
        }

        return null;
    }

    /// <summary>Tek anahtar için ayar değerini döndürür.</summary>
    public static string? Get(GatewayConfig config, string key)
        => config.Settings.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : null;

    /// <summary>Verilen değerlerin tamamının dolu olup olmadığını kontrol eder.</summary>
    public static bool AllPresent(params string?[] values)
        => values.All(v => !string.IsNullOrWhiteSpace(v));
}
