using System.ComponentModel.DataAnnotations;

namespace TriPay.Admin.Models.Gateways;

/// <summary>Gateway teknik ayar formu.</summary>
public sealed class GatewaySettingEditViewModel
{
    public int Id { get; set; }

    public int PaymentGatewayId { get; set; }

    public string GatewayCode { get; set; } = string.Empty;

    [Display(Name = "Ayar anahtarı")]
    public string SettingKey { get; set; } = string.Empty;

    [Display(Name = "Değer")]
    public string SettingValue { get; set; } = string.Empty;

    [Display(Name = "Ortam")]
    public string Environment { get; set; } = "All";

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;
}
