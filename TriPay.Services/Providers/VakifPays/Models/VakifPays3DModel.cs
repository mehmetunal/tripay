namespace TriPay.Services.Providers.VakifPays.Models;

/// <summary>
/// VakıfPayS 3D Secure ödeme ekranına otomatik form post edebilmek için gerekli adres ve alanları taşır.
/// </summary>
public sealed class VakifPays3DModel
{
    /// <summary>
    /// Kart bilgilerinin post edileceği VakıfPayS 3D endpoint adresidir.
    /// </summary>
    public string PostUrl { get; set; } = string.Empty;

    /// <summary>
    /// VakıfPayS 3D formuna hidden input olarak gönderilecek alanları tutar.
    /// </summary>
    public Dictionary<string, string> PostData { get; set; } = new();
}
