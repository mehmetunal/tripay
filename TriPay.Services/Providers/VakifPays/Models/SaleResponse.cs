namespace TriPay.Services.Providers.VakifPays.Models;

/// <summary>
/// VakıfPayS satış veya 3D dönüş sonucunu TriPay içinde standart okumak için kullanılan cevaptır.
/// </summary>
public class SaleResponse
{
    /// <summary>Bankadan gelen cevabın başarılı olup olmadığını belirtir.</summary>
    public bool Success { get; set; }

    /// <summary>Bankadan veya provider'dan gelen kullanıcıya gösterilebilir mesajdır.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Üye işyeri tarafındaki sipariş numarasıdır.</summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>VakıfPayS tarafındaki işlem numarasıdır.</summary>
    public string TransactionId { get; set; } = string.Empty;

    /// <summary>Cevabın HTML form yönlendirmesi içerip içermediğini belirtir.</summary>
    public bool RedirectHtml { get; set; }

    /// <summary>3D Secure ekranına yönlenilecek adresi taşır.</summary>
    public string RedirectUrl { get; set; } = string.Empty;

    /// <summary>VakıfPayS cevabının ham sözlük halidir; debug ve log için kullanılır.</summary>
    public Dictionary<string, object>? Raw { get; set; }
}
