using System.ComponentModel.DataAnnotations;

namespace TriPay.Admin.Models.Merchants;

/// <summary>Üye işyeri düzenleme.</summary>
public sealed class MerchantEditViewModel
{
    public int Id { get; set; }

    [Display(Name = "Ad")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Webhook URL")]
    public string? WebhookUrl { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; }

    [Display(Name = "API anahtarı (salt okunur)")]
    public string ApiKeyMasked { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
