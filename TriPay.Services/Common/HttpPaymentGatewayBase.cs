using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TriPay.Core.Gateways;
using TriPay.Core.Options;
using TriPay.Services.Common;
using TriPay.Services.Diagnostics;
using TriPay.Services.Models;

namespace TriPay.Core.Common;

/// <summary>HTTP tabanlı gateway provider'lar için ortak ayar okuma ve HTTP istek yardımcılarını sağlar.</summary>
public abstract class HttpPaymentGatewayBase : PaymentGatewayBase
{
    private readonly IGatewaySettingsProvider _settingsProvider;
    private readonly IHttpClientFactory _httpClientFactory;

    /// <summary>Ayar sağlayıcı ve HTTP client fabrikası ile taban sınıfı başlatır.</summary>
    protected HttpPaymentGatewayBase(
        IGatewaySettingsProvider settingsProvider,
        IHttpClientFactory httpClientFactory,
        ILogger logger) : base(logger)
    {
        _settingsProvider = settingsProvider;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>Geçerli gateway için yapılandırma kaydını döndürür.</summary>
    protected async Task<GatewayConfig?> GetGatewayConfigAsync(CancellationToken cancellationToken = default)
        => await _settingsProvider.GetGatewayConfigAsync(GatewayName, cancellationToken);

    /// <summary>Gateway yapılandırmasının etkin olup olmadığını kontrol eder.</summary>
    public override async Task<bool> IsSupportedAsync()
    {
        var config = await GetGatewayConfigAsync();
        return config is { Enabled: true };
    }

    /// <summary>Ham HTTP isteği gönderir ve yanıt gövdesini metin olarak döndürür.</summary>
    protected async Task<string> MakeRequestAsyncRaw(
        string url,
        HttpMethod method,
        string? content = null,
        Dictionary<string, string>? headers = null,
        string? contentType = "application/json",
        TimeSpan? timeout = null)
    {
        using var httpClient = _httpClientFactory.CreateClient();
        httpClient.Timeout = timeout ?? TimeSpan.FromSeconds(30);

        using var request = new HttpRequestMessage(method, url);

        if (headers != null)
        {
            foreach (var header in headers)
            {
                if (header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase)
                    || header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                    continue;
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            if (headers.TryGetValue("Authorization", out var auth))
                request.Headers.Authorization = AuthenticationHeaderValue.Parse(auth);
        }

        if (content != null)
            request.Content = new StringContent(content, Encoding.UTF8, contentType ?? "application/json");

        var response = await httpClient.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        PaymentDiagnostic.LogOutboundHttpPost(
            GatewayName,
            url,
            content,
            contentType,
            responseContent);

        if (!response.IsSuccessStatusCode)
        {
            Logger.LogWarning("{Gateway} HTTP {Status} {Url}", GatewayName, response.StatusCode, url);
        }

        return responseContent;
    }

    /// <summary>JSON gövdeli HTTP isteği gönderir ve yanıtı tip güvenli nesneye deserialize eder.</summary>
    protected async Task<T?> MakeRequestAsync<T>(
        string url,
        HttpMethod method,
        object? body = null,
        Dictionary<string, string>? headers = null,
        string? contentType = "application/json") where T : class
    {
        string? json = body == null
            ? null
            : JsonSerializer.Serialize(body, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });

        var raw = await MakeRequestAsyncRaw(url, method, json, headers, contentType);
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        return JsonSerializer.Deserialize<T>(raw, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    /// <summary>Form-urlencoded POST gönderir ve ham yanıt gövdesini döndürür (VakıfPayS vb.).</summary>
    protected async Task<string> MakeFormRequestAsync(
        string url,
        Dictionary<string, string> formFields,
        CancellationToken cancellationToken = default)
    {
        using var httpClient = _httpClientFactory.CreateClient();
        httpClient.Timeout = TimeSpan.FromSeconds(30);
        PaymentDiagnostic.LogOutbound3DForm(
            GatewayName,
            url,
            formFields,
            "application/x-www-form-urlencoded (API)");

        using var content = new FormUrlEncodedContent(formFields);
        using var response = await httpClient.PostAsync(url, content, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        PaymentDiagnostic.LogOutboundHttpPost(
            GatewayName,
            url,
            string.Join("&", formFields.Select(kv => $"{kv.Key}={kv.Value}")),
            "application/x-www-form-urlencoded",
            responseContent);

        if (!response.IsSuccessStatusCode)
            Logger.LogWarning("{Gateway} form HTTP {Status} {Url}", GatewayName, response.StatusCode, url);

        return responseContent;
    }
}
