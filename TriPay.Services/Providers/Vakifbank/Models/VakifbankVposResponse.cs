namespace TriPay.Services.Providers.Vakifbank.Models;

/// <summary>Vakıfbank VPOS XML yanıtından okunan sonuç alanları.</summary>
public sealed class VakifbankVposResponse
{
    /// <summary>VPOS sonuç kodu (0000 = başarılı).</summary>
    public string ResultCode { get; init; } = string.Empty;

    /// <summary>Banka sonuç açıklaması.</summary>
    public string ResultDetail { get; init; } = string.Empty;

    /// <summary>Banka işlem kimliği.</summary>
    public string? TransactionId { get; init; }

    /// <summary>İşlemin başarılı sayılıp sayılmadığı (DB'den gelen başarı kodu ile).</summary>
    public bool IsSuccessWithCode(string successCode)
        => string.Equals(ResultCode, successCode, StringComparison.Ordinal);
}
