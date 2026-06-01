using TriPay.Core.Gateways;

namespace TriPay.Core.Options;

/// <summary>appsettings.json içindeki TriPay kök bölümünün bağlandığı seçenek sınıfıdır.</summary>
public sealed class TriPayOptions
{
    /// <summary>Gateway belirtilmediğinde kullanılacak varsayılan kanal kodudur.</summary>
    public string DefaultGateway { get; set; } = PaymentGatewayNames.VakifPays;

    /// <summary>Gateway kodu → yapılandırma eşlemesidir.</summary>
    public Dictionary<string, GatewayConfig> Gateways { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Redis önbellek ayarları (<c>TriPay:Redis</c> — kayıt Infrastructure'da).</summary>
    public TriPayRedisOptions Redis { get; set; } = new();
}
