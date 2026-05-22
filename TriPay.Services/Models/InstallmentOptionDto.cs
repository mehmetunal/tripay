namespace TriPay.Services.Models;

/// <summary>Tek bir taksit seçeneğinin kullanıcıya gösterilecek özet bilgisidir.</summary>
public class InstallmentOptionDto
{
    /// <summary>Taksit sayısıdır (1 = tek çekim).</summary>
    public int Count { get; set; }

    /// <summary>Vade farkı veya komisyon oranı yüzdesidir.</summary>
    public decimal Rate { get; set; }

    /// <summary>Aylık ödeme tutarıdır.</summary>
    public decimal Monthly { get; set; }

    /// <summary>Toplam geri ödeme tutarıdır.</summary>
    public decimal Total { get; set; }

    /// <summary>UI'da gösterilecek etiket metnidir.</summary>
    public string Label { get; set; } = string.Empty;
}
