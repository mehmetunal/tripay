namespace TriPay.Data.Entities;

/// <summary>Ödeme kanalı tanımı (iyzico, Vakıfbank vb.).</summary>
public class PaymentGatewayRecord
{
    /// <summary>Birincil anahtar.</summary>
    public int Id { get; set; }

    /// <summary>PaymentGatewayNames ile uyumlu kanal kodu.</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Görünen ad.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Kanal aktif mi.</summary>
    public bool IsActive { get; set; } = true;
}
