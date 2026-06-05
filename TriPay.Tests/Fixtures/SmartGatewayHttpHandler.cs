using System.Net;
using System.Text;

namespace TriPay.Tests.Fixtures;

/// <summary>Protokole göre sahte HTTP yanıtları üreten test handler.</summary>
public sealed class SmartGatewayHttpHandler : HttpMessageHandler
{
    private readonly GatewayProtocolKind _protocol;

    /// <summary>Belirtilen protokol için handler oluşturur.</summary>
    public SmartGatewayHttpHandler(GatewayProtocolKind protocol)
        => _protocol = protocol;

    /// <summary>Protokol handler fabrikası.</summary>
    public static FakeHttpClientFactory CreateFactory(GatewayProtocolKind protocol)
        => new(new SmartGatewayHttpHandler(protocol));

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var url = request.RequestUri?.ToString() ?? string.Empty;
        var body = request.Content != null
            ? await request.Content.ReadAsStringAsync(cancellationToken)
            : string.Empty;

        var (content, mediaType) = ResolveResponse(url, body);
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(content, Encoding.UTF8, mediaType)
        };
    }

    private (string Content, string MediaType) ResolveResponse(string url, string body)
    {
        if (url.Contains("/api/token", StringComparison.OrdinalIgnoreCase))
            return ("""{"status_code":100,"data":{"token":"TEST-BEARER-TOKEN"}}""", "application/json");

        if (url.Contains("ahlpay", StringComparison.OrdinalIgnoreCase)
            && url.Contains("AuthenticationMerchant", StringComparison.OrdinalIgnoreCase))
        {
            return ("""{"isSuccess":true,"data":{"token":"ahlpay-token","tokenType":"Bearer","merchantId":"12345"}}""", "application/json");
        }

        if (url.Contains("ahlpay", StringComparison.OrdinalIgnoreCase))
            return ("""{"isSuccess":true,"data":"<html><body>3D</body></html>"}""", "application/json");

        if (url.Contains("paynet", StringComparison.OrdinalIgnoreCase))
            return ("""{"code":0,"html_content":"<html><body>3D</body></html>"}""", "application/json");

        if (url.Contains("moka", StringComparison.OrdinalIgnoreCase) || url.Contains("refmoka", StringComparison.OrdinalIgnoreCase))
            return ("""{"ResultCode":"Success","Data":{"Url":"https://3d.test/pay","CodeForHash":"HASH-1"}}""", "application/json");

        if (url.Contains("paynkolay", StringComparison.OrdinalIgnoreCase) || url.Contains("nkolay", StringComparison.OrdinalIgnoreCase))
        {
            return ("""{"RESPONSE_CODE":"2","USE_3D":"true","BANK_REQUEST_MESSAGE":"<html><body>3D</body></html>"}""", "application/json");
        }

        if (url.Contains("tami.com", StringComparison.OrdinalIgnoreCase))
        {
            var htmlB64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("<html><body>3D</body></html>"));
            return ($$"""{"success":true,"threeDSHtmlContent":"{{htmlB64}}"}""", "application/json");
        }

        if (url.Contains("ykb.com", StringComparison.OrdinalIgnoreCase)
            || body.Contains("posnetRequest", StringComparison.Ordinal))
        {
            if (body.Contains("oosResolveMerchantData", StringComparison.Ordinal))
                return ("<posnetResponse><approved>1</approved><respCode>00</respCode></posnetResponse>", "text/xml");

            return ("""
                <posnetResponse>
                  <approved>1</approved>
                  <data1>DATA1</data1>
                  <data2>DATA2</data2>
                  <sign>SIGN</sign>
                </posnetResponse>
                """, "text/xml");
        }

        if (url.Contains("param.com", StringComparison.OrdinalIgnoreCase) || body.Contains("TP_WMD_UCD", StringComparison.Ordinal))
        {
            return ("""
                <?xml version="1.0" encoding="utf-8"?>
                <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
                  <soap:Body>
                    <TP_WMD_UCDResponse xmlns="https://turkpos.com.tr/">
                      <TP_WMD_UCDResult>
                        <Sonuc>1</Sonuc>
                        <UCD_HTML><![CDATA[<html><body>3D</body></html>]]></UCD_HTML>
                      </TP_WMD_UCDResult>
                    </TP_WMD_UCDResponse>
                  </soap:Body>
                </soap:Envelope>
                """, "text/xml");
        }

        if (url.Contains("iyzipay.com", StringComparison.OrdinalIgnoreCase))
            return ("""{"status":"success","paymentId":"pay-test","threeDSHtmlContent":"<html>3D</html>"}""", "application/json");

        if (body.Contains("ACTION=REFUND", StringComparison.Ordinal)
            || body.Contains("REFUND", StringComparison.Ordinal) && body.Contains("PGTRANID", StringComparison.Ordinal))
            return ("""{"responseCode":"00","responseMsg":"Refunded"}""", "application/json");

        if (_protocol == GatewayProtocolKind.ApiV2
            || body.Contains("SESSIONTOKEN", StringComparison.Ordinal)
            || body.Contains("QUERYPAYMENTSYSTEMS", StringComparison.Ordinal))
        {
            if (body.Contains("QUERYPAYMENTSYSTEMS", StringComparison.Ordinal))
                return ("""{"responseCode":"00","installmentPaymentSystem":{"supports3D":"true","installmentList":[{"count":3,"customerCostCommissionRate":6}]}}""", "application/json");
            if (body.Contains("SESSIONTOKEN", StringComparison.Ordinal))
                return ("""{"responseCode":"00","sessionToken":"TEST-SESSION-TOKEN"}""", "application/json");
            if (body.Contains("ACTION=SALE", StringComparison.Ordinal))
                return ("""{"responseCode":"00","responseMsg":"Approved","pgTranId":"PG-TEST-1"}""", "application/json");
            if (body.Contains("QUERYTRANSACTION", StringComparison.Ordinal))
                return ("""{"responseCode":"00","responseMsg":"Approved"}""", "application/json");
            if (url.Contains("sale3d", StringComparison.OrdinalIgnoreCase))
                return ("<html><body><form>3D</form></body></html>", "text/html");
            return ("{}", "application/json");
        }

        if (_protocol == GatewayProtocolKind.Nestpay
            || body.Contains("storetype", StringComparison.OrdinalIgnoreCase))
        {
            if (body.Contains("DATA=", StringComparison.Ordinal))
                return ("<CC5Response><Response>Approved</Response><TransId>TX-1</TransId></CC5Response>", "text/xml");
            return ("<html><body><form><input name='Response' value='Approved'/></form></body></html>", "text/html");
        }

        if (_protocol == GatewayProtocolKind.CcPayment
            || url.Contains("paySmart3D", StringComparison.OrdinalIgnoreCase))
            return ("<html><body>3D Secure</body></html>", "text/html");

        if (body.Contains("GVPSRequest", StringComparison.Ordinal) || body.Contains("<GVPSRequest>", StringComparison.Ordinal))
            return ("<GVPSResponse><Transaction><Response><Message>Approved</Message><Code>00</Code></Response></Transaction></GVPSResponse>", "text/xml");

        return ("<html><body>3D</body></html>", "text/html");
    }
}
