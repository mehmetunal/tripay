using TriPay.Core.Gateways;
using TriPay.Services.Providers.Paratika;
using TriPay.Services.Providers.PaytenMsu;
using TriPay.Services.Providers.VakifPays;
using TriPay.Services.Providers.ZiraatPay;
using TriPay.Tests.Fixtures;

namespace TriPay.Tests.Unit.Gateways;

/// <summary>API v2 protokol katmanı testleri.</summary>
public sealed class ApiV2ProtocolTests
{
    public static IEnumerable<object[]> ApiV2Gateways()
    =>
    [
        [PaymentGatewayNames.VakifPays, typeof(VakifPaysGatewayProvider)],
        [PaymentGatewayNames.Paratika, typeof(ParatikaGatewayProvider)],
        [PaymentGatewayNames.PaytenMsu, typeof(PaytenMsuGatewayProvider)],
        [PaymentGatewayNames.ZiraatPay, typeof(ZiraatPayGatewayProvider)]
    ];

    [Theory]
    [MemberData(nameof(ApiV2Gateways))]
    public async Task GetInstallmentInfo_KomisyonHesaplanir(string gatewayName, Type _)
    {
        var provider = GatewayProviderTestFactory.Create(gatewayName);
        var result = await provider.GetInstallmentInfoAsync(new PaymentGatewayInstallmentRequestDto
        {
            CardNumber = "4938410157705590",
            Amount = 1000m,
            TestPlatform = true
        });

        Assert.True(result.IsSuccess);
        Assert.Contains(result.Data!.Installments, x => x.Count == 3);
    }

    [Theory]
    [MemberData(nameof(ApiV2Gateways))]
    public async Task GetPaymentStatus_Sorgu_Basarili(string gatewayName, Type _)
    {
        var provider = GatewayProviderTestFactory.Create(gatewayName);
        var result = await provider.GetPaymentStatusAsync("ORDER-TEST-1");
        Assert.True(result.IsSuccess);
    }

    [Theory]
    [MemberData(nameof(ApiV2Gateways))]
    public async Task RefundPayment_Refund_Basarili(string gatewayName, Type _)
    {
        var provider = GatewayProviderTestFactory.Create(gatewayName);
        var result = await provider.RefundPaymentAsync("PG-TEST-1", 10m);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("00", result.Data!.Raw?.GetValueOrDefault("responseCode")?.ToString());
    }

    [Theory]
    [InlineData(PaymentGatewayNames.Paratika)]
    [InlineData(PaymentGatewayNames.PaytenMsu)]
    [InlineData(PaymentGatewayNames.ZiraatPay)]
    public async Task ServerSide3D_InitializePayment_RedirectHtml_Dondurur(string gatewayName)
    {
        var provider = GatewayProviderTestFactory.Create(gatewayName);
        var result = await provider.InitializePaymentAsync(GatewayProviderTestFactory.CreateInitializeRequest());

        Assert.True(result.IsSuccess);
        Assert.False(string.IsNullOrWhiteSpace(result.Data!.RedirectHtml));
    }

    [Fact]
    public async Task VakifPays_InitializePayment_3D_AutoPostHtml_Dondurur()
    {
        var provider = GatewayProviderTestFactory.Create(PaymentGatewayNames.VakifPays);
        var result = await provider.InitializePaymentAsync(GatewayProviderTestFactory.CreateInitializeRequest());

        Assert.True(result.IsSuccess);
        Assert.Contains("form", result.Data!.RedirectHtml!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task VakifPays_InitializePayment_3DSizKart_SaleYapar()
    {
        var handler = new VakifPaysNon3DHttpHandler();
        var provider = GatewayProviderTestFactory.Create(
            PaymentGatewayNames.VakifPays,
            new FakeHttpClientFactory(handler));

        var result = await provider.InitializePaymentAsync(GatewayProviderTestFactory.CreateInitializeRequest());
        Assert.True(result.IsSuccess);
        Assert.Null(result.Data!.RedirectHtml);
    }

    private sealed class VakifPaysNon3DHttpHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var body = request.Content != null
                ? await request.Content.ReadAsStringAsync(cancellationToken)
                : string.Empty;

            var json = body.Contains("QUERYPAYMENTSYSTEMS", StringComparison.Ordinal)
                ? """{"responseCode":"00","installmentPaymentSystem":{"supports3D":"false"}}"""
                : body.Contains("ACTION=SALE", StringComparison.Ordinal)
                    ? """{"responseCode":"00","responseMsg":"Approved","pgTranId":"TX-2"}"""
                    : "{}";

            return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
            };
        }
    }
}
