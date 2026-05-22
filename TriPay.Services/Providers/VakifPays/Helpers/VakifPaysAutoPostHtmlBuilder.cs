using System.Net;
using System.Text;

namespace TriPay.Services.Providers.VakifPays.Helpers;

/// <summary>
/// VakıfPayS 3D Secure ekranına otomatik post edilecek HTML formunu güvenli şekilde üretir.
/// </summary>
public static class VakifPaysAutoPostHtmlBuilder
{
    /// <summary>
    /// Verilen URL ve form alanları ile tarayıcı açılır açılmaz submit olan HTML dökümanını döndürür.
    /// </summary>
    public static string Build(string url, IReadOnlyDictionary<string, string> data)
    {
        var safeUrl = WebUtility.HtmlEncode(url);
        var sb = new StringBuilder();
        sb.Append("<html><head><script>stringSubmit=function(){document.forms['vakifform'].submit();};</script></head>");
        sb.Append("<body onload='stringSubmit();'>");
        sb.Append($"<form id='vakifform' name='vakifform' action='{safeUrl}' method='POST'>");

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
