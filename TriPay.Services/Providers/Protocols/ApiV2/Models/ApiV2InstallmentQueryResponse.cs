namespace TriPay.Services.Providers.Protocols.ApiV2.Models;

/// <summary>API v2 BIN / taksit sorgusu cevabı.</summary>
public sealed class ApiV2InstallmentQueryResponse
{
    /// <summary>Sorgunun başarılı olup olmadığını belirtir.</summary>
    public bool Success { get; set; }

    /// <summary>Ham API cevabı.</summary>
    public Dictionary<string, object>? Raw { get; set; }
}
