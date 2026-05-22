namespace TriPay.Services.Models;

/// <summary>Auth3DS / ödeme tamamlama yanıtı; callback alanlarına ek kanal özgü alanlar içerir.</summary>
public class PaymentGatewayAuth3DSResponseDto : PaymentGatewayCallbackResponseDto
{
    /// <summary>Banka API status değeri (ör. success).</summary>
    public string? Status { get; set; }

    /// <summary>Iyzico fraud durum kodu.</summary>
    public int? FraudStatus { get; set; }

    /// <summary>Yerelleştirilmiş hata mesajı.</summary>
    public string? LocalizedErrorMessage { get; set; }

    /// <summary>Banka hata kodu.</summary>
    public string? ErrorCode { get; set; }

    /// <summary>İşlem tutarı (fiyat).</summary>
    public decimal? Price { get; set; }

    /// <summary>Seçilen taksit sayısı.</summary>
    public int? Installment { get; set; }

    /// <summary>Kart tipi.</summary>
    public string? CardType { get; set; }

    /// <summary>Kart markası/associasyon.</summary>
    public string? CardAssociation { get; set; }

    /// <summary>Kart ailesi.</summary>
    public string? CardFamily { get; set; }

    /// <summary>BIN numarası.</summary>
    public string? BinNumber { get; set; }

    /// <summary>Kart son dört hane.</summary>
    public string? LastFourDigits { get; set; }
}
