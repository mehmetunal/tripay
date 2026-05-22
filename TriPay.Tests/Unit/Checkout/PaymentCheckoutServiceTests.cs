using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TriPay.Data.Constants;
using TriPay.Data.DependencyInjection;
using TriPay.Data.Persistence;
using TriPay.Persistence.DependencyInjection;
using TriPay.Services;
using TriPay.Services.Checkout;
using TriPay.Services.Interfaces;
using TriPay.Services.Models;
using TriPay.Services.Providers.VakifPays.Models;
using TriPay.Tests.Fixtures;

namespace TriPay.Tests.Unit.Checkout;

/// <summary>DB tabanlı checkout orchestration unit testleri.</summary>
public class PaymentCheckoutServiceTests
{
    [Fact]
    public async Task PayAsync_IslemVePayRequestLoguOlusturur()
    {
        await using var provider = await CreateCheckoutProviderAsync();
        using var scope = provider.CreateScope();
        var checkout = scope.ServiceProvider.GetRequiredService<IPaymentCheckoutService>();
        var db = scope.ServiceProvider.GetRequiredService<TriPayDbContext>();

        var order = $"ORD-{Guid.NewGuid():N}"[..20];
        var result = await checkout.PayAsync(new PaymentRequest
        {
            OrderNumber = order,
            Amount = 100m,
            Currency = "TRY",
            CardNumber = "4938410000000006",
            ExpiryMonth = "12",
            ExpiryYear = "2030",
            Cvv = "123"
        }, PaymentGatewayNames.VakifPays);

        Assert.True(result.IsSuccess);
        var tx = await db.Transactions.FirstAsync(t => t.OrderNumber == order);
        Assert.Equal(TransactionStatuses.Pending, tx.Status);
        Assert.True(await db.TransactionLogs.AnyAsync(l =>
            l.TransactionId == tx.Id && l.LogType == TransactionLogTypes.PayRequest));
    }

    [Fact]
    public async Task ProcessCallbackAsync_TutarUyusmazligindaBasarisiz()
    {
        await using var provider = await CreateCheckoutProviderAsync();
        using var scope = provider.CreateScope();
        var checkout = scope.ServiceProvider.GetRequiredService<IPaymentCheckoutService>();

        var order = $"CB-{Guid.NewGuid():N}"[..18];
        await checkout.PayAsync(new PaymentRequest
        {
            OrderNumber = order,
            Amount = 250m,
            Currency = "TRY",
            CardNumber = "4938410000000006",
            ExpiryMonth = "12",
            ExpiryYear = "2030",
            Cvv = "123"
        }, PaymentGatewayNames.VakifPays);

        var outcome = await checkout.ProcessCallbackAsync(new Dictionary<string, string>
        {
            ["orderId"] = order,
            ["amount"] = "999.00",
            ["status"] = "success"
        }, PaymentGatewayNames.VakifPays);

        Assert.False(outcome.Success);
        Assert.Contains("uyuşmuyor", outcome.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<ServiceProvider> CreateCheckoutProviderAsync()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["TriPay:Database:UseInMemory"] = "true",
                ["TriPay:Persistence:Enabled"] = "true",
                ["TriPay:Persistence:PersistTransactionLogs"] = "true",
                ["ConnectionStrings:TriPay"] = "unused"
            })
            .Build();

        var provider = TestServiceProviderFactory.CreatePaymentServices(services =>
        {
            services.AddTriPayData(config);
            services.AddTriPayPersistence(config);
        });
        using var scope = provider.CreateScope();
        await TriPayDbSeed.EnsureDemoDataAsync(scope.ServiceProvider.GetRequiredService<TriPayDbContext>());
        return provider;
    }
}
