using System.ComponentModel.DataAnnotations;

namespace TriPay.Admin.Models.Gateways;

/// <summary>Gateway hata eşlemesi formu.</summary>
public sealed class GatewayErrorEditViewModel
{
    public int Id { get; set; }

    public int PaymentGatewayId { get; set; }

    public string GatewayCode { get; set; } = string.Empty;

    [Display(Name = "Provider hata kodu")]
    public string ProviderErrorCode { get; set; } = string.Empty;

    [Display(Name = "Normalize kod")]
    public string? NormalizedCode { get; set; }

    [Display(Name = "Kullanıcı mesajı")]
    public string UserMessage { get; set; } = string.Empty;

    [Display(Name = "Dil")]
    public string Locale { get; set; } = "tr";

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;
}
