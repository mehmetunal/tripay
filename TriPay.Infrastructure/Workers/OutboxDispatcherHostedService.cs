using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TriPay.Data.Repositories;
using TriPay.Infrastructure.Configuration;
using TriPay.Infrastructure.Messaging;

namespace TriPay.Infrastructure.Workers;

/// <summary>Outbox tablosundan RabbitMQ'ya webhook mesajı yayınlar (Faz 1.2).</summary>
public sealed class OutboxDispatcherHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TriPayRabbitMqOptions _options;
    private readonly ILogger<OutboxDispatcherHostedService> _logger;

    /// <summary>Hosted service oluşturur.</summary>
    public OutboxDispatcherHostedService(
        IServiceScopeFactory scopeFactory,
        IOptions<TriPayRabbitMqOptions> options,
        ILogger<OutboxDispatcherHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>Periyodik outbox taraması.</summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Outbox dispatcher devre dışı (TriPay:RabbitMq:Enabled=false).");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DispatchBatchAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Outbox dispatcher döngüsünde hata.");
            }

            await Task.Delay(TimeSpan.FromSeconds(Math.Max(1, _options.PollIntervalSeconds)), stoppingToken);
        }
    }

    private async Task DispatchBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IPaymentTransactionRepository>();
        var publisher = scope.ServiceProvider.GetRequiredService<RabbitMqOutboxPublisher>();
        var pending = await repo.GetPendingOutboxAsync(20, cancellationToken);

        foreach (var msg in pending)
        {
            try
            {
                await publisher.PublishAsync(msg.RoutingKey, msg.Payload, cancellationToken);
                await repo.MarkOutboxPublishedAsync(msg.Id, cancellationToken);
                _logger.LogInformation("Outbox {OutboxId} RabbitMQ'ya yayınlandı.", msg.Id);
            }
            catch (Exception ex)
            {
                await repo.IncrementOutboxRetryAsync(msg.Id, cancellationToken);
                _logger.LogWarning(ex, "Outbox {OutboxId} yayınlanamadı, retry artırıldı.", msg.Id);
            }
        }
    }
}
