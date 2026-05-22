namespace TriPay.Services.Models;

/// <summary>Taksit sorgusu sonucunda dönen seçenek listesini taşır.</summary>
public class InstallmentInfoResponse
{
    /// <summary>Sorgunun başarılı olup olmadığını belirtir.</summary>
    public bool Success { get; set; }

    /// <summary><see cref="Success"/> ile aynı; eski API uyumluluğu.</summary>
    public bool IsSuccess { get => Success; set => Success = value; }

    /// <summary>Banka hata kodu.</summary>
    public string? ErrorCode { get; set; }

    /// <summary>Banka hata mesajı.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Yerelleştirilmiş hata mesajı.</summary>
    public string? LocalizedErrorMessage { get; set; }

    /// <summary>Kullanıcıya sunulacak taksit seçenekleri listesidir.</summary>
    public List<InstallmentOptionDto> Installments { get; set; } = new();
}
