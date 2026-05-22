namespace TriPay.Services.Providers.VakifPays.Models;

/// <summary>
/// VakıfPayS işlem durum sorgusu cevabını taşır.
/// </summary>
public sealed class SaleQueryResponse
{
    /// <summary>Sorgunun bankada başarılı sonuçlanıp sonuçlanmadığını belirtir.</summary>
    public bool Success { get; set; }

    /// <summary>Bankadan dönen açıklama mesajıdır.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>VakıfPayS işlem sorgusunun ham cevabıdır.</summary>
    public Dictionary<string, object>? Raw { get; set; }
}
