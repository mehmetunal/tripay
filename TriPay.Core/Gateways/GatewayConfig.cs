namespace TriPay.Core.Gateways;

/// <summary>Tek bir ödeme kanalının appsettings veya veritabanından okunan yapılandırmasıdır.</summary>
public sealed class GatewayConfig
{
    /// <summary>Kanalın uygulamada kullanılıp kullanılmayacağını belirtir.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Test (sandbox) ortamının seçilip seçilmediğini belirtir.</summary>
    public bool IsTestMode { get; set; } = true;

    /// <summary>MerchantId, ApiKey vb. kanala özgü anahtar-değer ayarlarıdır.</summary>
    public Dictionary<string, string> Settings { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
