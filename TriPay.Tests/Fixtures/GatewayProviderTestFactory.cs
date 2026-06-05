using Microsoft.Extensions.Logging;
using TriPay.Core.Gateways;
using TriPay.Services.Interfaces;
using TriPay.Infrastructure.Gateways;
using TriPay.Infrastructure.Redis;
using TriPay.Services.Providers.Vakifbank;

namespace TriPay.Tests.Fixtures;

/// <summary>Gateway provider test örnekleri oluşturur.</summary>
public static class GatewayProviderTestFactory
{
    /// <summary>Gateway adına göre provider örneği oluşturur.</summary>
    public static IPaymentGatewayProvider Create(
        string gatewayName,
        IHttpClientFactory? httpClientFactory = null,
        bool enabled = true)
    {
        var entry = GatewayProviderTestCatalog.All.First(x =>
            string.Equals(x.GatewayName, gatewayName, StringComparison.OrdinalIgnoreCase));

        var settings = new FakeGatewaySettings(entry.GatewayName, new Dictionary<string, string>(entry.Settings), enabled);
        var http = httpClientFactory ?? SmartGatewayHttpHandler.CreateFactory(entry.Protocol);
        var logger = CreateLogger(entry.ProviderType);

        if (entry.ProviderType == typeof(VakifbankGatewayProvider))
        {
            return new VakifbankGatewayProvider(
                settings,
                http,
                new InMemoryVakifbankSaleStateStore(),
                InMemoryGatewayMetadataService.CreateWithVakifbankDefaults(),
                (ILogger<VakifbankGatewayProvider>)logger);
        }

        var instance = Activator.CreateInstance(entry.ProviderType, settings, http, logger)
                       ?? throw new InvalidOperationException($"{entry.ProviderType.Name} oluşturulamadı.");
        return (IPaymentGatewayProvider)instance;
    }

    /// <summary>Standart test ödeme isteği oluşturur.</summary>
    public static PaymentGatewayInitializeRequestDto CreateInitializeRequest(bool testPlatform = true)
        => new()
        {
            Payment = new TriPay.Services.Providers.VakifPays.Models.PaymentRequest
            {
                CardNumber = "4938410157705590",
                Amount = 10m,
                OrderNumber = $"ORD-{Guid.NewGuid():N}"[..16],
                TestPlatform = testPlatform,
                ExpiryMonth = "12",
                ExpiryYear = "2030",
                Cvv = "123",
                CardOwner = "Test User",
                CustomerName = "Test User",
                CustomerEmail = "test@test.com",
                CustomerIp = "127.0.0.1",
                CustomerPhone = "5555555555",
                ReturnUrl = "https://merchant.test/callback",
                Currency = "TRY",
                InstallmentCount = 1
            }
        };

    private static object CreateLogger(Type providerType)
    {
        var loggerType = typeof(TestLogger<>).MakeGenericType(providerType);
        return Activator.CreateInstance(loggerType)!;
    }
}
