namespace TriPay.Services.Providers.Common;

/// <summary>Form tabanlı gateway yanıt ayrıştırma yardımcıları.</summary>
public static class FormGatewayResponseHelper
{
    /// <summary><c>;;</c> ile ayrılmış key=value yanıtını sözlüğe çevirir.</summary>
    public static Dictionary<string, string> ParseDelimitedResponse(string response, string delimiter = ";;")
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(response))
            return result;

        foreach (var part in response.Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries))
        {
            var idx = part.IndexOf('=');
            if (idx <= 0)
                continue;
            result[part[..idx]] = part[(idx + 1)..];
        }

        return result;
    }

    /// <summary>Ham callback sözlüğünden büyük/küçük harf duyarsız değer okur.</summary>
    public static string? GetRaw(IReadOnlyDictionary<string, string> data, string key)
    {
        foreach (var kv in data)
        {
            if (kv.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
                return kv.Value;
        }
        return null;
    }
}
