using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using TriPay.Infrastructure.Configuration;

namespace TriPay.Infrastructure.Messaging;

/// <summary>Outbox mesajını RabbitMQ exchange'e yayınlar.</summary>
public sealed class RabbitMqOutboxPublisher : IDisposable
{
    private readonly TriPayRabbitMqOptions _options;
    private readonly ILogger<RabbitMqOutboxPublisher> _logger;
    private IConnection? _connection;
    private IChannel? _channel;

    /// <summary>Publisher oluşturur.</summary>
    public RabbitMqOutboxPublisher(IOptions<TriPayRabbitMqOptions> options, ILogger<RabbitMqOutboxPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>Mesajı routing key ile yayınlar.</summary>
    public async Task PublishAsync(string routingKey, string payload, CancellationToken cancellationToken = default)
    {
        await EnsureChannelAsync(cancellationToken);
        if (_channel == null)
            throw new InvalidOperationException("RabbitMQ kanalı oluşturulamadı.");

        var body = Encoding.UTF8.GetBytes(payload);
        var props = new BasicProperties { ContentType = "application/json", DeliveryMode = DeliveryModes.Persistent };
        await _channel.BasicPublishAsync(_options.ExchangeName, routingKey, false, props, body, cancellationToken);
    }

    private async Task EnsureChannelAsync(CancellationToken cancellationToken)
    {
        if (_channel != null && _channel.IsOpen)
            return;

        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password
        };

        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
        await _channel.ExchangeDeclareAsync(_options.ExchangeName, ExchangeType.Topic, durable: true, cancellationToken: cancellationToken);
    }

    /// <summary>Bağlantıları kapatır.</summary>
    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
