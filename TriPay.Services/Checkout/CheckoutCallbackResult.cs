namespace TriPay.Services.Checkout;

/// <summary>Callback işleme sonucu (view model için).</summary>
public sealed class CheckoutCallbackResult
{
    /// <summary>Ödeme başarılı ve doğrulandı mı.</summary>
    public bool Success { get; init; }

    /// <summary>Kullanıcıya gösterilecek mesaj.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>Sipariş numarası.</summary>
    public string OrderNumber { get; init; } = string.Empty;

    /// <summary>Banka işlem kimliği.</summary>
    public string? TransactionId { get; init; }

    /// <summary>Callback yanıt kodu.</summary>
    public string ResponseCode { get; init; } = string.Empty;

    /// <summary>Callback mesajı.</summary>
    public string CallbackMessage { get; init; } = string.Empty;

    /// <summary>Hata mesajı.</summary>
    public string ErrorMessage { get; init; } = string.Empty;

    /// <summary>Sorgu yanıt kodu.</summary>
    public string QueryResponseCode { get; init; } = string.Empty;

    /// <summary>Ham callback alanları (gösterim).</summary>
    public string CallbackFieldsDisplay { get; init; } = string.Empty;

    /// <summary>Callback tutar metni.</summary>
    public string AmountText { get; init; } = string.Empty;
}
