namespace TriPay.Core.Options;

/// <summary>TriPay MSSQL kalıcılık ve log politikası (Hosted mod).</summary>
public sealed class TriPayPersistenceOptions
{
    /// <summary>Yapılandırma bölümü: <c>TriPay:Persistence</c>.</summary>
    public const string SectionName = "TriPay:Persistence";

    /// <summary>
    /// TriPay veritabanı ve <c>IPaymentCheckoutService</c> aktif mi?
    /// Framework (NuGet) modunda <c>false</c> — KVKK riski üye işyerinde kalır.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary><c>TransactionLogs</c> tablosuna maskeli ham log yazılsın mı?</summary>
    public bool PersistTransactionLogs { get; set; } = true;

    /// <summary>Başarılı ödeme sonrası <c>OutboxMessages</c> / webhook kuyruğu.</summary>
    public bool EnableOutbox { get; set; } = true;
}
