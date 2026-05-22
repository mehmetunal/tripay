namespace TriPay.Tests.Integration;

/// <summary>TriPay web uygulaması health endpoint entegrasyon testleri.</summary>
[Trait("Category", "Integration")]
public sealed class TriPayWebHealthIntegrationTests : IClassFixture<TriPayWebApplicationFactory>
{
    private readonly HttpClient _client;

    /// <summary>Test web fabrikası ile HTTP istemcisi oluşturur.</summary>
    public TriPayWebHealthIntegrationTests(TriPayWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthLive_200Dondurur()
    {
        var response = await _client.GetAsync("/health/live");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task HealthReady_200Dondurur()
    {
        var response = await _client.GetAsync("/health/ready");
        response.EnsureSuccessStatusCode();
    }
}
