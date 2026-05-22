namespace TriPay.Services.Providers.Iyzico.Helpers;

/// <summary>Iyzico fraud ve merchant onay kurallarını merkezileştirir.</summary>
public static class IyzicoPaymentHelper
{
    /// <summary>Fraud onaylı durum kodu.</summary>
    public const int FraudApproved = 1;

    /// <summary>Fraud inceleme altında durum kodu.</summary>
    public const int FraudUnderReview = 0;

    /// <summary>Fraud reddedildi durum kodu.</summary>
    public const int FraudDeclined = -1;

    /// <summary>İnceleme sonrası onay durum kodu.</summary>
    public const int FraudApprovedAfterReview = 2;

    /// <summary>Status, paymentStatus ve fraudStatus değerlerine göre işlemin merchant onaylı sayılıp sayılmadığını döner.</summary>
    public static bool IsMerchantApproved(string? status, string? paymentStatus, int? fraudStatus)
    {
        if (!string.Equals(status, "success", StringComparison.OrdinalIgnoreCase))
            return false;

        if (!string.IsNullOrWhiteSpace(paymentStatus)
            && !string.Equals(paymentStatus, "SUCCESS", StringComparison.OrdinalIgnoreCase))
            return false;

        if (!fraudStatus.HasValue)
            return true;

        return fraudStatus.Value is FraudApproved or FraudApprovedAfterReview;
    }

    /// <summary>Fraud durum koduna göre son kullanıcıya gösterilecek Türkçe mesajı döndürür.</summary>
    public static string GetFraudUserMessage(int? fraudStatus)
    {
        if (fraudStatus == FraudUnderReview)
            return "Ödemeniz güvenlik incelemesine alındı.";

        if (fraudStatus == FraudDeclined)
            return "Ödemeniz güvenlik kontrolünden geçemedi.";

        return "Ödeme tamamlanamadı.";
    }
}
