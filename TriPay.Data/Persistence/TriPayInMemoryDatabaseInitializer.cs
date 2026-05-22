using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TriPay.Data.Persistence;

/// <summary>InMemory veritabanı için şema ve seed oluşturur (test/demo).</summary>
public sealed class TriPayInMemoryDatabaseInitializer : IHostedService
{
    private readonly IServiceProvider _services;

    /// <summary>Initializer oluşturur.</summary>
    public TriPayInMemoryDatabaseInitializer(IServiceProvider services) => _services = services;

    /// <summary>Uygulama başlangıcında şema ve seed uygular.</summary>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TriPayDbContext>();
        await TriPayDbSeed.EnsureDemoDataAsync(db, cancellationToken);
    }

    /// <summary>Durdurma işlemi yok.</summary>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
