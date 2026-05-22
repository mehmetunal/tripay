namespace TriPay.Tests.Fixtures;

/// <summary>Özelleştirilebilir <see cref="HttpMessageHandler"/> ile HTTP client fabrikası.</summary>
public sealed class FakeHttpClientFactory : IHttpClientFactory
{
    private readonly HttpMessageHandler _handler;

    /// <summary>Varsayılan boş 200 yanıtı ile fabrika oluşturur.</summary>
    public FakeHttpClientFactory()
        : this(new FakeHttpMessageHandler(string.Empty))
    {
    }

    /// <summary>Belirtilen handler ile fabrika oluşturur.</summary>
    public FakeHttpClientFactory(HttpMessageHandler handler)
    {
        _handler = handler;
    }

    /// <summary>Sabit handler ile yapılandırılmış HttpClient örneği döndürür.</summary>
    public HttpClient CreateClient(string name) => new(_handler, disposeHandler: false);
}
