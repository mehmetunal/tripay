namespace TriPay.Core.Redis;

/// <summary>TriPay Redis mantıksal anahtar kategorileri (InstanceName öneki ayrı eklenir).</summary>
public static class RedisKeyNames
{
    /// <summary>Vakıfbank 3D satış durumu: <c>vakifbank:sale:{orderCode}</c></summary>
    public const string VakifbankSalePrefix = "vakifbank:sale:";

    /// <summary>Idempotency: <c>idempotency:{key}</c></summary>
    public const string IdempotencyPrefix = "idempotency:";

    /// <summary>Dağıtık kilit: <c>lock:txn:{transactionId}</c></summary>
    public const string LockPrefix = "lock:txn:";

    /// <summary>Rate limit: <c>rl:{merchantId}</c></summary>
    public const string RateLimitPrefix = "rl:";

    /// <summary>Checkout oturum tutarı: <c>checkout:amount:{merchantId}:{orderNumber}</c></summary>
    public const string CheckoutAmountPrefix = "checkout:amount:";

    /// <summary>Gateway ayarları: <c>gateway:settings:{code}:{env}</c></summary>
    public const string GatewaySettingsPrefix = "gateway:settings:";

    /// <summary>Gateway hataları: <c>gateway:errors:{code}:{locale}</c></summary>
    public const string GatewayErrorsPrefix = "gateway:errors:";

    /// <summary>Gateway ayar önbellek anahtarı.</summary>
    public static string GatewaySettings(string gatewayCode, string environment)
        => GatewaySettingsPrefix + gatewayCode + ":" + environment;

    /// <summary>Gateway hata önbellek anahtarı.</summary>
    public static string GatewayErrors(string gatewayCode, string locale)
        => GatewayErrorsPrefix + gatewayCode + ":" + locale;

    /// <summary>Vakıfbank satış anahtarı üretir.</summary>
    public static string VakifbankSale(string orderCode) => VakifbankSalePrefix + orderCode;

    /// <summary>Idempotency anahtarı üretir.</summary>
    public static string Idempotency(string key) => IdempotencyPrefix + key;

    /// <summary>İşlem kilidi anahtarı üretir.</summary>
    public static string TransactionLock(int transactionId) => LockPrefix + transactionId;

    /// <summary>Üye işyeri rate limit anahtarı üretir.</summary>
    public static string RateLimit(int merchantId) => RateLimitPrefix + merchantId;
}
