using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TriPay.Tests.Integration;

/// <summary>Web entegrasyon testleri — Redis bellek içi modda.</summary>
public sealed class TriPayWebApplicationFactory : WebApplicationFactory<Program>
{
    /// <summary>Test ortamında Redis devre dışı (InMemory cache).</summary>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["TriPay:Redis:Enabled"] = "false",
                ["TriPay:RabbitMq:Enabled"] = "false",
                ["TriPay:Database:UseInMemory"] = "true"
            });
        });
    }
}
