using System.Text.Json;
using Newtonsoft.Json;

namespace TriPay.Services.Providers.Common;

/// <summary>Gateway JSON yanıt ayrıştırma yardımcıları.</summary>
public static class JsonResponseHelper
{
    /// <summary>System.Text.Json ile ham JSON gövdesini sözlüğe çevirir.</summary>
    public static Dictionary<string, object?> ParseDictionary(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return new Dictionary<string, object?>();

        return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object?>>(raw)
               ?? new Dictionary<string, object?>();
    }

    /// <summary>Newtonsoft.Json ile ham JSON gövdesini sözlüğe çevirir (Payten form yanıtları).</summary>
    public static Dictionary<string, object> ParseNewtonsoftDictionary(string raw)
        => JsonConvert.DeserializeObject<Dictionary<string, object>>(raw) ?? new Dictionary<string, object>();
}
