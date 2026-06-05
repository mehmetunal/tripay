using TriPay.Core.Gateways;
using TriPay.Tests.Fixtures;

namespace TriPay.Tests.Unit.Gateways;

/// <summary>Tüm kayıtlı POS gateway provider'ları için ortak smoke ve davranış testleri.</summary>
public sealed class AllGatewayProvidersTests
{
    public static IEnumerable<object[]> AllGateways()
        => GatewayProviderTestCatalog.All.Select(x => new object[] { x.GatewayName });

    [Theory]
    [MemberData(nameof(AllGateways))]
    public void KayitliProvider_GatewayName_Eslesir(string gatewayName)
    {
        var provider = GatewayProviderTestFactory.Create(gatewayName);
        Assert.Equal(gatewayName, provider.GatewayName, StringComparer.OrdinalIgnoreCase);
    }

    [Theory]
    [MemberData(nameof(AllGateways))]
    public void KayitliProvider_DisplayName_Dolu(string gatewayName)
    {
        var provider = GatewayProviderTestFactory.Create(gatewayName);
        Assert.False(string.IsNullOrWhiteSpace(provider.DisplayName));
    }

    [Theory]
    [MemberData(nameof(AllGateways))]
    public async Task KayitliProvider_IsSupported_AktifAyar_ile_True(string gatewayName)
    {
        var provider = GatewayProviderTestFactory.Create(gatewayName);
        Assert.True(await provider.IsSupportedAsync());
    }

    [Theory]
    [MemberData(nameof(AllGateways))]
    public async Task KayitliProvider_IsSupported_PasifAyar_ile_False(string gatewayName)
    {
        var provider = GatewayProviderTestFactory.Create(gatewayName, enabled: false);
        Assert.False(await provider.IsSupportedAsync());
    }

    [Theory]
    [MemberData(nameof(AllGateways))]
    public async Task KayitliProvider_ProcessCallback_OrnekVeri_ile_Basarili(string gatewayName)
    {
        var entry = GatewayProviderTestCatalog.All.First(x =>
            string.Equals(x.GatewayName, gatewayName, StringComparison.OrdinalIgnoreCase));
        var provider = GatewayProviderTestFactory.Create(gatewayName);

        var result = await provider.ProcessCallbackAsync(entry.CallbackRequest);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.True(result.Data!.Success);
    }

    [Theory]
    [MemberData(nameof(AllGateways))]
    public void KayitliProvider_NormalizeCallback_HataFirlatmaz(string gatewayName)
    {
        var entry = GatewayProviderTestCatalog.All.First(x =>
            string.Equals(x.GatewayName, gatewayName, StringComparison.OrdinalIgnoreCase));
        var provider = GatewayProviderTestFactory.Create(gatewayName);

        var normalized = provider.NormalizeCallbackFromRawData(
            entry.CallbackRequest.RawData.Count > 0
                ? entry.CallbackRequest.RawData
                : new Dictionary<string, string> { ["probe"] = "1" });

        _ = normalized;
        Assert.True(true);
    }

    [Fact]
    public void KayitliProvider_Sayisi_38()
        => Assert.Equal(38, GatewayProviderTestCatalog.All.Count);
}
