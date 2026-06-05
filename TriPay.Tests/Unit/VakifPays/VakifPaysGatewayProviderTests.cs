using System.Net;
using System.Text;
using TriPay.Services;
using TriPay.Services.Models;
using TriPay.Services.Providers.VakifPays;
using TriPay.Services.Providers.Common;
using TriPay.Tests.Fixtures;

namespace TriPay.Tests.Unit.VakifPays;

/// <summary>VakıfPayS gateway provider unit testleri (HttpPaymentGatewayBase deseni).</summary>
public sealed class VakifPaysGatewayProviderTests
{
    [Fact]
    public async Task ProcessCallback_BasariliAlanlar_MapEdilir()
    {
        var provider = CreateProvider();
        var result = await provider.ProcessCallbackAsync(new PaymentGatewayCallbackRequestDto
        {
            RawData = new Dictionary<string, string>
            {
                ["responseCode"] = "00",
                ["merchantPaymentId"] = "ORDER-1",
                ["pgTranId"] = "TX-1"
            }
        });

        Assert.True(result.IsSuccess);
        Assert.Equal("ORDER-1", result.Data!.OrderNumber);
        Assert.Equal("TX-1", result.Data.TransactionId);
    }

    [Fact]
    public async Task GetInstallmentInfo_KomisyonOrani_Yansir()
    {
        var installmentJson = """
            {
              "responseCode": "00",
              "installmentPaymentSystem": {
                "installmentList": [ { "count": 3, "customerCostCommissionRate": 6 } ]
              }
            }
            """;

        var handler = new FakeHttpMessageHandler((_, body) =>
            body.Contains("QUERYPAYMENTSYSTEMS", StringComparison.Ordinal)
                ? installmentJson
                : "{}");

        var provider = CreateProvider(handler);
        var result = await provider.GetInstallmentInfoAsync(new PaymentGatewayInstallmentRequestDto
        {
            CardNumber = "4938410157705590",
            Amount = 1000m,
            TestPlatform = true
        });

        Assert.True(result.IsSuccess);
        var inst = result.Data!.Installments.Single(x => x.Count == 3);
        Assert.Equal(1060m, inst.Total);
        Assert.Equal(6m, inst.Rate);
    }

    [Fact]
    public async Task InitializePayment_3DSizKart_SaleCagrilir()
    {
        var handler = new FakeHttpMessageHandler((_, body) =>
        {
            if (body.Contains("QUERYPAYMENTSYSTEMS", StringComparison.Ordinal))
                return """{"responseCode":"00","installmentPaymentSystem":{"supports3D":"false"}}""";
            if (body.Contains("ACTION=SALE", StringComparison.Ordinal))
                return """{"responseCode":"00","responseMsg":"Approved","pgTranId":"TX-2"}""";
            return """{"responseCode":"99"}""";
        });

        var provider = CreateProvider(handler);
        var result = await provider.InitializePaymentAsync(new PaymentGatewayInitializeRequestDto
        {
            Payment = new PaymentRequest
            {
                CardNumber = "4938410157705590",
                Amount = 10m,
                OrderNumber = "ORDER-2",
                TestPlatform = true,
                ExpiryMonth = "12",
                ExpiryYear = "2030",
                Cvv = "123",
                CardOwner = "Test User"
            }
        });

        Assert.True(result.IsSuccess);
        Assert.Null(result.Data!.RedirectHtml);
    }

    [Fact]
    public void AutoPostHtml_EncodeEder()
    {
        var html = PaymentAutoPostHtmlBuilder.Build("https://bank.test/pay?a=1&b=2", new Dictionary<string, string>
        {
            ["cardOwner"] = "A' <B>"
        });

        Assert.Contains("a=1&amp;b=2", html);
        Assert.Contains("A&#39; &lt;B&gt;", html);
    }

    private static VakifPaysGatewayProvider CreateProvider(FakeHttpMessageHandler? handler = null)
    {
        var http = handler ?? new FakeHttpMessageHandler("{}");
        return new VakifPaysGatewayProvider(
            new FakeGatewaySettings(PaymentGatewayNames.VakifPays, DefaultSettings()),
            new FakeHttpClientFactory(http),
            new TestLogger<VakifPaysGatewayProvider>());
    }

    private static Dictionary<string, string> DefaultSettings()
        => new(StringComparer.OrdinalIgnoreCase)
        {
            ["Merchant"] = "10009011",
            ["MerchantUser"] = "apitest48@vakifpays.com.tr",
            ["MerchantPassword"] = "Api.123.1234"
        };

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, string, string> _responseFactory;

        public FakeHttpMessageHandler(Func<HttpRequestMessage, string, string> responseFactory)
            => _responseFactory = responseFactory;

        public FakeHttpMessageHandler(string staticBody)
            => _responseFactory = (_, _) => staticBody;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var body = request.Content != null
                ? await request.Content.ReadAsStringAsync(cancellationToken)
                : string.Empty;
            var json = _responseFactory(request, body);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        }
    }
}
