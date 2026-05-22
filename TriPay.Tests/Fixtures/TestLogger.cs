using Microsoft.Extensions.Logging;

namespace TriPay.Tests.Fixtures;

/// <summary>Testlerde sessiz <see cref="ILogger{T}"/> implementasyonu.</summary>
public sealed class TestLogger<T> : ILogger<T>
{
    /// <summary>Log kapsamı oluşturmaz.</summary>
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    /// <summary>Tüm log seviyeleri kapalıdır.</summary>
    public bool IsEnabled(LogLevel logLevel) => false;

    /// <summary>Log yazmaz.</summary>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
    }
}
