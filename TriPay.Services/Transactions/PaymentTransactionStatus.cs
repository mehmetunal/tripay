namespace TriPay.Services.Transactions;

/// <summary>MSSQL <c>Transactions.Status</c> alanı için bağlayıcı işlem durumları.</summary>
public enum PaymentTransactionStatus
{
    /// <summary>Kayıt oluşturuldu; bankaya henüz istek gitmedi.</summary>
    Created = 0,

    /// <summary>3D Secure yönlendirmesi bekleniyor.</summary>
    Pending3D = 1,

    /// <summary>Banka callback alındı; Auth3DS/VPOS bekleniyor olabilir.</summary>
    CallbackReceived = 2,

    /// <summary>Banka tarafında işlem devam ediyor.</summary>
    Processing = 3,

    /// <summary>Tahsilat başarılı (terminal).</summary>
    Success = 4,

    /// <summary>İşlem reddedildi veya hata (terminal).</summary>
    Failed = 5,

    /// <summary>İptal veya süre aşımı (terminal).</summary>
    Cancelled = 6,

    /// <summary>İade tamamlandı (terminal).</summary>
    Refunded = 7
}
