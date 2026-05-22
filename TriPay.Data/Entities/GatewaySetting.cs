namespace TriPay.Data.Entities;

/// <summary>Ödeme kanalı teknik ayarı (URL, durum kodu vb.).</summary>
public class GatewaySetting
{
    /// <summary>Birincil anahtar.</summary>
    public int Id { get; set; }

    /// <summary>FK → PaymentGateways.</summary>
    public int PaymentGatewayId { get; set; }

    /// <summary>Ayar anahtarı (<see cref="TriPay.Core.Gateways.GatewaySettingKeys"/>).</summary>
    public string SettingKey { get; set; } = string.Empty;

    /// <summary>Ayar değeri.</summary>
    public string SettingValue { get; set; } = string.Empty;

    /// <summary>Ortam: All, Test, Production.</summary>
    public string Environment { get; set; } = "All";

    /// <summary>Kayıt aktif mi.</summary>
    public bool IsActive { get; set; } = true;
}
