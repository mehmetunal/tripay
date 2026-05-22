namespace TriPay.Infrastructure.Configuration;

/// <summary>RabbitMQ bağlantı ayarları.</summary>
public sealed class TriPayRabbitMqOptions
{
    /// <summary>Yapılandırma bölüm adı.</summary>
    public const string SectionName = "TriPay:RabbitMq";

    /// <summary>Outbox dispatcher etkin mi.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>AMQP URI (ör. amqp://guest:guest@localhost:5672/).</summary>
    public string HostName { get; set; } = "localhost";

    /// <summary>Port.</summary>
    public int Port { get; set; } = 5672;

    /// <summary>Kullanıcı adı.</summary>
    public string UserName { get; set; } = "guest";

    /// <summary>Şifre.</summary>
    public string Password { get; set; } = "guest";

    /// <summary>Exchange adı.</summary>
    public string ExchangeName { get; set; } = "tripay.events";

    /// <summary>Polling aralığı (saniye).</summary>
    public int PollIntervalSeconds { get; set; } = 5;
}
