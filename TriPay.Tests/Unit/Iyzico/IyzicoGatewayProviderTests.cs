using TriPay.Services.Models;
using TriPay.Services.Providers.Iyzico;
using TriPay.Tests.Fixtures;

namespace TriPay.Tests.Unit.Iyzico;

/// <summary>Iyzico gateway provider unit testleri.</summary>
public sealed class IyzicoGatewayProviderTests
{
    [Fact]
    public async Task ProcessCallback_BasariliAlanlar_MapEdilir()
    {
        var provider = CreateProvider();
        var result = await provider.ProcessCallbackAsync(new PaymentGatewayCallbackRequestDto
        {
            IsSuccess = true,
            PaymentId = "pay-1",
            ConversationId = "order-1",
            PaymentStatus = "SUCCESS"
        });

        Assert.True(result.IsSuccess);
        Assert.Equal("order-1", result.Data!.OrderNumber);
    }

    [Fact]
    public void NormalizeCallbackFromRawData_SuccessStatus_Dogrulanir()
    {
        var provider = CreateProvider();
        var normalized = provider.NormalizeCallbackFromRawData(new Dictionary<string, string>
        {
            ["status"] = "success",
            ["paymentId"] = "P1",
            ["conversationId"] = "O1"
        });

        Assert.Equal("success", normalized.Status);
        Assert.Equal("P1", normalized.PaymentId);
        Assert.Equal("O1", normalized.ConversationId);
    }

    private static IyzicoGatewayProvider CreateProvider()
        => new(
            new FakeGatewaySettings(PaymentGatewayNames.Iyzico, new Dictionary<string, string>
            {
                ["ApiKey"] = "key",
                ["SecretKey"] = "secret"
            }),
            new FakeHttpClientFactory(),
            new TestLogger<IyzicoGatewayProvider>());
}
