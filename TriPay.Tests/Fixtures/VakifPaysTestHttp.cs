using System.Net;
using System.Text;

namespace TriPay.Tests.Fixtures;

/// <summary>Checkout ve integration testleri için VakıfPayS sahte HTTP yanıtları.</summary>
public static class VakifPaysTestHttp
{
    /// <summary>3D initialize ve sorgu akışları için varsayılan başarılı yanıtlar döndürür.</summary>
    public static FakeHttpClientFactory CreateClientFactory()
        => new(new VakifPaysTestMessageHandler());

    private sealed class VakifPaysTestMessageHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var body = request.Content != null
                ? await request.Content.ReadAsStringAsync(cancellationToken)
                : string.Empty;

            var json = body.Contains("QUERYPAYMENTSYSTEMS", StringComparison.Ordinal)
                ? """{"responseCode":"00","installmentPaymentSystem":{"supports3D":"true"}}"""
                : body.Contains("SESSIONTOKEN", StringComparison.Ordinal)
                    ? """{"responseCode":"00","sessionToken":"TEST-SESSION-TOKEN"}"""
                    : body.Contains("ACTION=SALE", StringComparison.Ordinal)
                        ? """{"responseCode":"00","responseMsg":"Approved","pgTranId":"PG-TEST-1"}"""
                        : body.Contains("QUERYTRANSACTION", StringComparison.Ordinal)
                            ? """{"responseCode":"00","responseMsg":"Approved"}"""
                            : "{}";

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        }
    }
}
