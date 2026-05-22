namespace TriPay.Data.Entities;

/// <summary>TriPay üye işyeri kaydı.</summary>
public class Merchant
{
    /// <summary>Birincil anahtar.</summary>
    public int Id { get; set; }

    /// <summary>Üye işyeri adı.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>API anahtarı (demo).</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Webhook bildirim URL'si.</summary>
    public string? WebhookUrl { get; set; }

    /// <summary>Kayıt aktif mi.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Oluşturulma zamanı (UTC).</summary>
    public DateTime CreatedAt { get; set; }
}
