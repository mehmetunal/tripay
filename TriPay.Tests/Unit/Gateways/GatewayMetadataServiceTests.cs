using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TriPay.Core.Gateways;
using TriPay.Core.Options;
using TriPay.Core.Redis;
using TriPay.Data.DependencyInjection;
using TriPay.Data.Persistence;
using TriPay.Infrastructure.Gateways;
using TriPay.Infrastructure.Redis;

namespace TriPay.Tests.Unit.Gateways;

/// <summary>Gateway metadata DB + Redis önbellek testleri.</summary>
public sealed class GatewayMetadataServiceTests
{
    [Fact]
    public async Task GetSetting_VakifbankEnrollmentUrl_DatabaseDenGelir()
    {
        await using var provider = await CreateProviderWithSeedAsync();
        using var scope = provider.CreateScope();
        var metadata = scope.ServiceProvider.GetRequiredService<IGatewayMetadataService>();

        var url = await metadata.GetSettingAsync("Vakifbank", GatewaySettingKeys.EnrollmentUrl, isTestMode: true);

        Assert.Equal("https://3dsecuretest.vakifbank.com.tr/MPIAPI/MPI_Enrollment.aspx", url);
    }

    [Fact]
    public async Task GetErrorMessage_BilininenKod_TurkceMesaj()
    {
        await using var provider = await CreateProviderWithSeedAsync();
        using var scope = provider.CreateScope();
        var metadata = scope.ServiceProvider.GetRequiredService<IGatewayMetadataService>();

        var msg = await metadata.GetErrorMessageAsync("Vakifbank", "0051");

        Assert.Equal("Limit yetersiz", msg);
    }

    private static async Task<ServiceProvider> CreateProviderWithSeedAsync()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["TriPay:Database:UseInMemory"] = "true",
                ["TriPay:Redis:Enabled"] = "false",
                ["ConnectionStrings:TriPay"] = "unused"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddTriPayData(config);
        services.AddDistributedMemoryCache();
        services.Configure<TriPayRedisOptions>(o => o.GatewayMetadataCacheMinutes = 60);
        services.AddSingleton<ITriPayRedisCache>(sp => new TriPayRedisCache(sp.GetRequiredService<Microsoft.Extensions.Caching.Distributed.IDistributedCache>(), null));
        services.AddScoped<IGatewayMetadataService, RedisCachedGatewayMetadataService>();

        var sp = services.BuildServiceProvider();
        using var scope = sp.CreateScope();
        await TriPayDbSeed.EnsureDemoDataAsync(scope.ServiceProvider.GetRequiredService<TriPayDbContext>());
        return sp;
    }
}
