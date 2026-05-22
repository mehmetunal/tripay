namespace TriPay.Services.Providers.VakifPays.Models;

/// <summary>
/// Demo ödeme ekranından ve provider katmanından VakıfPayS ödeme başlatmak için kullanılan kart, müşteri ve adres bilgilerini taşır.
/// </summary>
public sealed class PaymentRequest
{
    /// <summary>İşlemin VakıfPayS test ortamında çalışıp çalışmayacağını belirler.</summary>
    public bool TestPlatform { get; set; } = true;

    /// <summary>Kart destekliyorsa 3D Secure akışının kullanılmasını sağlar.</summary>
    public bool Use3D { get; set; } = true;

    /// <summary>Üye işyeri tarafındaki benzersiz sipariş numarasıdır.</summary>
    public string OrderNumber { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>Kart üzerindeki ad soyad bilgisidir.</summary>
    public string CardOwner { get; set; } = string.Empty;

    /// <summary>Boşluksuz veya boşluklu kart numarasıdır; provider bankaya sadece rakamları gönderir.</summary>
    public string CardNumber { get; set; } = string.Empty;

    /// <summary>Kart son kullanma ayıdır.</summary>
    public string ExpiryMonth { get; set; } = string.Empty;

    /// <summary>Kart son kullanma yılıdır.</summary>
    public string ExpiryYear { get; set; } = string.Empty;

    /// <summary>Kart güvenlik kodudur; sistemde kalıcı olarak saklanmamalıdır.</summary>
    public string Cvv { get; set; } = string.Empty;

    /// <summary>Tahsil edilecek işlem tutarıdır.</summary>
    public decimal Amount { get; set; }

    /// <summary>Seçilen taksit sayısıdır; tek çekim için 1 olmalıdır.</summary>
    public int InstallmentCount { get; set; } = 1;

    /// <summary>Para birimi kodudur.</summary>
    public string Currency { get; set; } = "TRY";

    /// <summary>Üye işyeri tarafındaki müşteri kimliğidir.</summary>
    public string CustomerId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Müşterinin ad soyad bilgisidir.</summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>Müşterinin e-posta adresidir.</summary>
    public string CustomerEmail { get; set; } = string.Empty;

    /// <summary>Müşterinin ödeme başlattığı IP adresidir.</summary>
    public string CustomerIp { get; set; } = "127.0.0.1";

    /// <summary>Müşterinin telefon numarasıdır.</summary>
    public string CustomerPhone { get; set; } = string.Empty;

    /// <summary>3D Secure sonrası VakıfPayS'in sonucu post edeceği dönüş adresidir.</summary>
    public string ReturnUrl { get; set; } = string.Empty;

    /// <summary>Fatura adres satırıdır.</summary>
    public string BillToAddressLine { get; set; } = string.Empty;

    /// <summary>Fatura şehir bilgisidir.</summary>
    public string BillToCity { get; set; } = string.Empty;

    /// <summary>Fatura ülke bilgisidir.</summary>
    public string BillToCountry { get; set; } = "Turkey";

    /// <summary>Fatura posta kodudur.</summary>
    public string BillToPostalCode { get; set; } = string.Empty;

    /// <summary>Fatura telefon numarasıdır.</summary>
    public string BillToPhone { get; set; } = string.Empty;

    /// <summary>Teslimat adres satırıdır.</summary>
    public string ShipToAddressLine { get; set; } = string.Empty;

    /// <summary>Teslimat şehir bilgisidir.</summary>
    public string ShipToCity { get; set; } = string.Empty;

    /// <summary>Teslimat ülke bilgisidir.</summary>
    public string ShipToCountry { get; set; } = "Turkey";

    /// <summary>Teslimat posta kodudur.</summary>
    public string ShipToPostalCode { get; set; } = string.Empty;

    /// <summary>Teslimat telefon numarasıdır.</summary>
    public string ShipToPhone { get; set; } = string.Empty;
}
