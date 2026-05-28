using System.Text;

namespace TriPay.Services.Diagnostics;

/// <summary>
/// Ödeme / 3D / callback ham verilerini tanılama hedeflerine yazar.
/// Demo'da ekranda göstermek için <see cref="IPaymentDiagnosticSink"/> kaydedin.
/// </summary>
public static class PaymentDiagnostic
{
    private static readonly List<IPaymentDiagnosticSink> Sinks = new();
    private static readonly object SinkLock = new();

    /// <summary>true ise kayıtlar sink'lere yazılır.</summary>
    public static bool Enabled { get; set; }

    /// <summary>Tanılama hedefi ekler.</summary>
    public static void RegisterSink(IPaymentDiagnosticSink sink)
    {
        lock (SinkLock)
            Sinks.Add(sink);
    }

    /// <summary>Tarayıcının banka 3D sayfasına POST edeceği form alanları.</summary>
    public static void LogOutbound3DForm(
        string gateway,
        string postUrl,
        IReadOnlyDictionary<string, string>? formFields,
        string? note = null)
    {
        if (!Enabled) return;

        var sb = new StringBuilder();
        sb.AppendLine($"POST URL: {postUrl}");
        if (!string.IsNullOrWhiteSpace(note))
            sb.AppendLine($"Not: {note}");
        sb.AppendLine("--- Form alanları (tarayıcı → banka) ---");
        AppendFields(sb, formFields);

        Emit("3D Giden", gateway, "3D Secure form POST", sb.ToString());
    }

    /// <summary>Sunucudan banka API'sine giden HTTP POST.</summary>
    public static void LogOutboundHttpPost(
        string gateway,
        string url,
        string? requestBody,
        string? contentType = null,
        string? responseBody = null)
    {
        if (!Enabled) return;

        var sb = new StringBuilder();
        sb.AppendLine($"URL: {url}");
        if (!string.IsNullOrWhiteSpace(contentType))
            sb.AppendLine($"Content-Type: {contentType}");
        sb.AppendLine("--- İstek gövdesi ---");
        sb.AppendLine(string.IsNullOrWhiteSpace(requestBody) ? "(boş)" : requestBody);
        if (responseBody != null)
        {
            sb.AppendLine("--- Yanıt gövdesi ---");
            sb.AppendLine(string.IsNullOrWhiteSpace(responseBody) ? "(boş)" : responseBody);
        }

        Emit("API Giden", gateway, "Banka API HTTP POST", sb.ToString());
    }

    /// <summary>Bankadan dönen callback POST formu.</summary>
    public static void LogInboundCallback(
        string gateway,
        IReadOnlyDictionary<string, string> formFields,
        string? source = null)
    {
        if (!Enabled) return;

        var sb = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(source))
            sb.AppendLine($"Kaynak: {source}");
        sb.AppendLine("--- Form alanları (banka → merchant) ---");
        AppendFields(sb, formFields);

        Emit("Callback Gelen", gateway, "Banka callback POST", sb.ToString());
    }

    /// <summary>Checkout sayfasından gelen ödeme formu.</summary>
    public static void LogCheckoutPayRequest(string gateway, IReadOnlyDictionary<string, string?> fields)
    {
        if (!Enabled) return;

        var sb = new StringBuilder();
        foreach (var kv in fields.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase))
            sb.AppendLine($"{kv.Key} = {kv.Value ?? ""}");

        Emit("Checkout", gateway, "Ödeme formu gönderildi", sb.ToString());
    }

    /// <summary>3D auto-submit HTML yanıtı.</summary>
    public static void LogHtmlResponse(string gateway, string html)
    {
        if (!Enabled) return;
        Emit("3D HTML", gateway, "Tarayıcıya dönen yönlendirme HTML", html);
    }

    private static void Emit(string category, string gateway, string title, string detail)
    {
        var entry = new PaymentDiagnosticEntry
        {
            Category = category,
            Gateway = gateway,
            Title = title,
            Detail = detail,
            CorrelationId = PaymentDiagnosticContext.CurrentOrderNumber
        };

        IPaymentDiagnosticSink[] snapshot;
        lock (SinkLock)
            snapshot = Sinks.ToArray();

        foreach (var sink in snapshot)
            sink.Write(entry);
    }

    private static void AppendFields(StringBuilder sb, IReadOnlyDictionary<string, string>? fields)
    {
        if (fields is { Count: > 0 })
        {
            foreach (var kv in fields.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase))
                sb.AppendLine($"{kv.Key} = {kv.Value}");
        }
        else
            sb.AppendLine("(alan yok)");
    }
}
