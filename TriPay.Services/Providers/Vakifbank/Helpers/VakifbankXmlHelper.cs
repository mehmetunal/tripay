using System.Xml;
using TriPay.Services.Providers.Vakifbank.Models;

namespace TriPay.Services.Providers.Vakifbank.Helpers;

/// <summary>Vakıfbank XML yanıtlarını parse eden yardımcılar.</summary>
public static class VakifbankXmlHelper
{
    private const string VposResponseResultCodePath = "VposResponse/ResultCode";
    private const string VposResponseResultDetailPath = "VposResponse/ResultDetail";
    private const string VposResponseTransactionIdPath = "VposResponse/TransactionId";

    /// <summary>VPOS XML yanıtından sonuç alanlarını okur.</summary>
    public static VakifbankVposResponse ParseVposResponse(XmlDocument doc)
    {
        var resultCode = doc.SelectSingleNode(VposResponseResultCodePath)?.InnerText?.Trim() ?? string.Empty;
        var resultDetail = doc.SelectSingleNode(VposResponseResultDetailPath)?.InnerText?.Trim() ?? string.Empty;
        var transactionId = doc.SelectSingleNode(VposResponseTransactionIdPath)?.InnerText?.Trim();
        return new VakifbankVposResponse
        {
            ResultCode = resultCode,
            ResultDetail = resultDetail,
            TransactionId = transactionId
        };
    }
}
