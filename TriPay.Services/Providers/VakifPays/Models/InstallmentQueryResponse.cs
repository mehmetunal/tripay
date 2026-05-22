namespace TriPay.Services.Providers.VakifPays.Models;

/// <summary>
/// VakıfPayS BIN veya tüm taksit sorgusu cevabını taşır.
/// </summary>
public sealed class InstallmentQueryResponse
{
    /// <summary>Sorgunun bankada başarılı sonuçlanıp sonuçlanmadığını belirtir.</summary>
    public bool Success { get; set; }

    /// <summary>VakıfPayS'in döndürdüğü ham taksit cevabıdır.</summary>
    public Dictionary<string, object>? Raw { get; set; }
}
