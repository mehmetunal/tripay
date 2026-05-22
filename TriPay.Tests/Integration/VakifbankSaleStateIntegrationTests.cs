using TriPay.Core.Vakifbank;
using TriPay.Infrastructure.Gateways;
using TriPay.Services.Models;
using TriPay.Services.Providers.Vakifbank;
using TriPay.Services.Providers.Vakifbank.Models;
using TriPay.Tests.Fixtures;

namespace TriPay.Tests.Integration;

/// <summary>Vakıfbank 3D satış durumu + Auth3DS entegrasyon testleri.</summary>
[Trait("Category", "Integration")]
public sealed class VakifbankSaleStateIntegrationTests
{
    [Fact]
    public async Task Auth3DS_SaleStateKayitli_VposBasarili()
    {
        var store = new TriPay.Infrastructure.Redis.InMemoryVakifbankSaleStateStore();
        await store.SetAsync("ORD-AUTH3D", new VakifbankSaleState
        {
            OrderCode = "ORD-AUTH3D",
            Cvv = "999",
            ClientIp = "127.0.0.1",
            ExpiryYYYYMM = "203012",
            PurchaseAmount = "250.00",
            CurrencyCode = "949"
        });

        var provider = new VakifbankGatewayProvider(
            new FakeGatewaySettings(PaymentGatewayNames.Vakifbank, new Dictionary<string, string>
            {
                ["MerchantId"] = "m",
                ["MerchantPassword"] = "p",
                ["TerminalNo"] = "t",
                ["EnrollmentUrl"] = "https://test/enroll",
                ["VerifyUrl"] = "https://test/vpos",
                ["ResultCodeSuccess"] = "0000",
                ["ThreeDsStatusEnrolled"] = "Y",
                ["ThreeDsStatusAttempt"] = "A"
            }),
            new FakeHttpClientFactory(new FakeHttpMessageHandler(VakifbankTestXml.VposSuccess)),
            store,
            InMemoryGatewayMetadataService.CreateWithVakifbankDefaults(),
            new TestLogger<VakifbankGatewayProvider>());

        var auth = await provider.Auth3DSAsync(new PaymentGatewayAuth3DSRequestDto
        {
            ConversationId = "ORD-AUTH3D",
            RawData = new Dictionary<string, string>
            {
                ["Status"] = "Y",
                ["VerifyEnrollmentRequestId"] = "mpi-1",
                ["SessionInfo"] = "ORD-AUTH3D",
                ["Pan"] = "4938410157705590",
                ["Eci"] = "05",
                ["Cavv"] = "AA=="
            }
        });

        Assert.True(auth.IsSuccess);
        Assert.Equal("VPOS-TX-99", auth.Data!.PaymentId);
    }
}
