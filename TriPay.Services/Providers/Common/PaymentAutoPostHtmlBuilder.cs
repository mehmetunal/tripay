using System.Net;
using System.Text;

namespace TriPay.Services.Providers.Common;

/// <summary>3D Secure ekranına otomatik POST edilecek HTML formunu güvenli şekilde üretir.</summary>
public static class PaymentAutoPostHtmlBuilder
{
    /// <summary>Verilen URL ve form alanları ile tarayıcı açılır açılmaz submit olan HTML dökümanını döndürür.</summary>
    public static string Build(string url, IReadOnlyDictionary<string, string> data, string formId = "tripayform")
    {
        var safeUrl = WebUtility.HtmlEncode(url);
        var sb = new StringBuilder();
        sb.Append("<html><head><script>function submitForm(){document.forms['").Append(formId).Append("'].submit();}</script></head>");
        sb.Append("<body onload='submitForm();'>");
        sb.Append($"<form id='{formId}' name='{formId}' action='{safeUrl}' method='POST'>");

        foreach (var item in data)
        {
            var safeName = WebUtility.HtmlEncode(item.Key);
            var safeValue = WebUtility.HtmlEncode(item.Value);
            sb.Append($"<input type='hidden' name='{safeName}' value='{safeValue}' />");
        }

        sb.Append("</form></body></html>");
        return sb.ToString();
    }
}
