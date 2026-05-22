namespace TriPay.Core.Vakifbank;

/// <summary>3D MPI enrollment ile VPOS satışı arasında Redis'te tutulan geçici ödeme durumudur.</summary>
public sealed class VakifbankSaleState
{
    /// <summary>Üye işyeri sipariş veya session kodudur.</summary>
    public string OrderCode { get; set; } = string.Empty;

    /// <summary>Kart CVV değeri (kısa süreli; PCI kurallarına uygun saklama süresi uygulanmalıdır).</summary>
    public string Cvv { get; set; } = string.Empty;

    /// <summary>Müşteri IP adresi.</summary>
    public string ClientIp { get; set; } = string.Empty;

    /// <summary>Kart son kullanma YYYYMM formatı.</summary>
    public string ExpiryYYYYMM { get; set; } = string.Empty;

    /// <summary>Enrollment'da gönderilen tutar (VPOS ile aynı olmalıdır).</summary>
    public string PurchaseAmount { get; set; } = string.Empty;

    /// <summary>ISO sayısal para birimi kodu (ör. 949 = TRY).</summary>
    public string CurrencyCode { get; set; } = "949";
}
