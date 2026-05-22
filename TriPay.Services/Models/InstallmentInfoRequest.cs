namespace TriPay.Services.Models;

/// <summary>Taksit sorgusu için kart ve tutar bilgilerini taşıyan temel istek modelidir.</summary>
public class InstallmentInfoRequest
{
    /// <summary>Tam kart numarası veya maskelenmiş kart değeridir.</summary>
    public string CardNumber { get; set; } = string.Empty;

    /// <summary>Yalnızca BIN (ilk 6 hane) ile sorgu yapılacaksa kullanılır.</summary>
    public string? BinNumber { get; set; }

    /// <summary>Sipariş veya işlem tutarıdır.</summary>
    public decimal Amount { get; set; }

    /// <summary><see cref="Amount"/> ile aynı alan; eski API uyumluluğu için.</summary>
    public decimal Price { get => Amount; set => Amount = value; }

    /// <summary>API locale (ör. tr).</summary>
    public string Locale { get; set; } = "tr";

    /// <summary>İsteğe bağlı conversation / sipariş kimliği.</summary>
    public string? ConversationId { get; set; }

    /// <summary>Para birimi kodudur (ör. TRY).</summary>
    public string Currency { get; set; } = "TRY";

    /// <summary>Hedef gateway kodu; boşsa varsayılan kanal kullanılır.</summary>
    public string? GatewayName { get; set; }

    /// <summary>Test ortamı bayrağıdır (kanala göre yorumlanır).</summary>
    public bool TestPlatform { get; set; } = true;
}
