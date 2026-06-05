using System.Globalization;
using System.Text;
using System.Xml;

namespace TriPay.Services.Providers.Nestpay.Helpers;

/// <summary>Nestpay CC5 XML istek/yanıt dönüşümleri.</summary>
public static class NestpayXmlHelper
{
    /// <summary>Sözlükten CC5Request XML gövdesi üretir.</summary>
    public static string ToXml(IReadOnlyDictionary<string, object?> parameters, string rootTag = "CC5Request")
    {
        var sb = new StringBuilder();
        sb.Append('<').Append(rootTag).Append('>');
        AppendElements(sb, parameters);
        sb.Append("</").Append(rootTag).Append('>');
        return sb.ToString();
    }

    /// <summary>XML yanıtını düz sözlüğe çevirir.</summary>
    public static Dictionary<string, string> ParseResponse(string xml, string rootTag = "CC5Response")
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(xml))
            return result;

        var doc = new XmlDocument();
        doc.LoadXml(xml);

        var root = doc.GetElementsByTagName(rootTag).Item(0) ?? doc.DocumentElement;
        if (root == null)
            return result;

        foreach (XmlNode child in root.ChildNodes)
        {
            if (child.NodeType != XmlNodeType.Element)
                continue;

            if (child.Name.Equals("Extra", StringComparison.OrdinalIgnoreCase))
            {
                foreach (XmlNode extraChild in child.ChildNodes)
                {
                    if (extraChild.NodeType == XmlNodeType.Element && extraChild.FirstChild != null)
                        result[$"Extra.{extraChild.Name}"] = extraChild.InnerText.Trim();
                }
                continue;
            }

            result[child.Name] = child.InnerText.Trim();
        }

        return result;
    }

    /// <summary>Tutarı Nestpay formatına çevirir (1234.56 → 1234.56).</summary>
    public static string FormatAmount(decimal amount)
        => amount.ToString("N2", CultureInfo.GetCultureInfo("tr-TR"))
            .Replace(".", "")
            .Replace(",", ".");

    private static void AppendElements(StringBuilder sb, IReadOnlyDictionary<string, object?> parameters)
    {
        foreach (var (key, value) in parameters)
        {
            if (value == null)
                continue;

            if (value is IReadOnlyDictionary<string, object?> nested)
            {
                sb.Append('<').Append(key).Append('>');
                AppendElements(sb, nested);
                sb.Append("</").Append(key).Append('>');
                continue;
            }

            sb.Append('<').Append(key).Append('>')
                .Append(XmlEscape(value.ToString() ?? string.Empty))
                .Append("</").Append(key).Append('>');
        }
    }

    private static string XmlEscape(string value)
        => value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");

    /// <summary>HTML yanıtından hidden input alanlarını çıkarır.</summary>
    public static Dictionary<string, string> ParseFormFields(string html)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(html))
            return result;

        var pattern = "name=[\"']([^\"']+)[\"'][^>]*value=[\"']([^\"']*)[\"']";
        foreach (System.Text.RegularExpressions.Match match in
                 System.Text.RegularExpressions.Regex.Matches(html, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
        {
            result[match.Groups[1].Value] = match.Groups[2].Value;
        }

        return result;
    }
}
