namespace TriPay.Services.Providers.Protocols.ApiV2.Models;

/// <summary>API v2 işlem durum sorgusu cevabı.</summary>
public sealed class ApiV2SaleQueryResponse
{
    /// <summary>Sorgunun başarılı olup olmadığını belirtir.</summary>
    public bool Success { get; set; }

    /// <summary>API açıklama mesajı.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Ham API cevabı.</summary>
    public Dictionary<string, object>? Raw { get; set; }
}
