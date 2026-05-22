namespace TriPay.Demo.Models;

/// <summary>Hata sayfasında gösterilecek istek izleme bilgisini taşır.</summary>
public class ErrorViewModel
{
    /// <summary>İstek veya aktivite izleme kimliği.</summary>
    public string? RequestId { get; set; }

    /// <summary>İstek kimliğinin kullanıcıya gösterilip gösterilmeyeceğini belirler.</summary>
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
