namespace TriPay.Data.Entities;

/// <summary>Provider hata kodu → kullanıcı mesajı eşlemesi.</summary>
public class GatewayErrorMapping
{
    /// <summary>Birincil anahtar.</summary>
    public int Id { get; set; }

    /// <summary>FK → PaymentGateways.</summary>
    public int PaymentGatewayId { get; set; }

    /// <summary>Banka/provider ham hata kodu.</summary>
    public string ProviderErrorCode { get; set; } = string.Empty;

    /// <summary>Normalize TriPay kodu (opsiyonel).</summary>
    public string? NormalizedCode { get; set; }

    /// <summary>Kullanıcıya gösterilecek mesaj.</summary>
    public string UserMessage { get; set; } = string.Empty;

    /// <summary>Dil (tr, en).</summary>
    public string Locale { get; set; } = "tr";

    /// <summary>Kayıt aktif mi.</summary>
    public bool IsActive { get; set; } = true;
}
