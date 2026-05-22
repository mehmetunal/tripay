namespace TriPay.Core.Options;

/// <summary>TriPay Redis önbellek yapılandırması (<c>TriPay:Redis</c> bölümü).</summary>
public sealed class TriPayRedisOptions
{
    /// <summary>Yapılandırma bölüm adı.</summary>
    public const string SectionName = "TriPay:Redis";

    /// <summary>Redis etkin mi (false ise bellek içi cache).</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>StackExchange.Redis bağlantı dizesi.</summary>
    public string Configuration { get; set; } = string.Empty;

    /// <summary><see cref="Microsoft.Extensions.Caching.Distributed.IDistributedCache"/> anahtar öneki.</summary>
    public string InstanceName { get; set; } = "tripay:";

    /// <summary>Vakıfbank 3D satış durumu TTL (saat).</summary>
    public int SaleStateTtlHours { get; set; } = 24;

    /// <summary>Idempotency kayıt TTL (gün).</summary>
    public int IdempotencyTtlDays { get; set; } = 7;

    /// <summary>Initialize idempotency TTL (saat).</summary>
    public int InitializeIdempotencyTtlHours { get; set; } = 24;

    /// <summary>Dağıtık kilit TTL (saniye).</summary>
    public int DistributedLockSeconds { get; set; } = 30;

    /// <summary>Rate limit pencere süresi (saniye).</summary>
    public int RateLimitWindowSeconds { get; set; } = 60;

    /// <summary>Rate limit — pencere başına maksimum istek.</summary>
    public int RateLimitMaxRequests { get; set; } = 120;

    /// <summary>Gateway metadata (ayar + hata) önbellek TTL (dakika).</summary>
    public int GatewayMetadataCacheMinutes { get; set; } = 60;
}
