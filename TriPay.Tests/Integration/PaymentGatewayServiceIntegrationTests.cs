using TriPay.Services.Interfaces;
using TriPay.Services.Models;
using TriPay.Tests.Fixtures;

namespace TriPay.Tests.Integration;

/// <summary><see cref="IPaymentGatewayService"/> entegrasyon testleri (DI + idempotency).</summary>
[Trait("Category", "Integration")]
public sealed class PaymentGatewayServiceIntegrationTests
{
    [Fact]
    public async Task ProcessCallback_IkinciCagri_IdempotentSonuc()
    {
        using var sp = TestServiceProviderFactory.CreatePaymentServices();
        var payment = sp.GetRequiredService<IPaymentGatewayService>();

        var request = new PaymentGatewayCallbackRequestDto
        {
            GatewayName = PaymentGatewayNames.Vakifbank,
            RawData = new Dictionary<string, string>
            {
                ["Status"] = "Y",
                ["VerifyEnrollmentRequestId"] = "idem-req-1",
                ["SessionInfo"] = "idem-order-1",
                ["PurchAmount"] = "100.00",
                ["PurchCurrency"] = "949"
            }
        };

        var first = await payment.ProcessCallbackAsync(request);
        var second = await payment.ProcessCallbackAsync(request);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.Equal(first.Data!.OrderNumber, second.Data!.OrderNumber);
    }

    [Fact]
    public async Task GetActiveGateways_EnAzBirAktif()
    {
        using var sp = TestServiceProviderFactory.CreatePaymentServices();
        var payment = sp.GetRequiredService<IPaymentGatewayService>();
        var gateways = await payment.GetActiveGatewaysAsync();
        Assert.NotEmpty(gateways);
    }
}
