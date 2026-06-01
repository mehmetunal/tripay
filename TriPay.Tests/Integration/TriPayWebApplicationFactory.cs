using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace TriPay.Tests.Integration;

/// <summary>Demo web entegrasyon testleri — Redis/RabbitMQ kapalı (bellek içi).</summary>
public sealed class TriPayWebApplicationFactory : WebApplicationFactory<Program>
{
    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            // appsettings.json'dan sonra eklenir; test değerleri kazanır
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["TriPay:Redis:Enabled"] = "false",
                ["TriPay:RabbitMq:Enabled"] = "false",
                ["TriPay:Database:UseInMemory"] = "true"
            });
        });
    }
}
