namespace TriPay.Tests.Fixtures;

/// <summary>HTTP isteklerini test içinde sabit yanıtla karşılayan handler.</summary>
public sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

    /// <summary>İstek başına yanıt üreten fonksiyon ile handler oluşturur.</summary>
    public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
    {
        _handler = handler;
    }

    /// <summary>Tüm isteklere aynı gövdeyi döndüren handler oluşturur.</summary>
    public FakeHttpMessageHandler(string responseBody, string contentType = "text/xml")
        : this(_ => new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(responseBody, System.Text.Encoding.UTF8, contentType)
        })
    {
    }

    /// <summary>Testte tanımlı yanıt üreticisini çağırır.</summary>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => Task.FromResult(_handler(request));
}
