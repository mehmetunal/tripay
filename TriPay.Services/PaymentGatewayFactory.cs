using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TriPay.Core.Gateways;
using TriPay.Core.Options;
using TriPay.Services.DependencyInjection;
using TriPay.Services.Interfaces;

namespace TriPay.Services;

/// <summary>Gateway koduna göre doğru <see cref="IPaymentGatewayProvider"/> örneğini DI'dan çözen fabrikadır.</summary>
public class PaymentGatewayFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptionsMonitor<TriPayOptions> _options;
    private readonly Dictionary<string, Type> _providers =
        PaymentGatewayProviderRegistration.ProviderTypes.ToDictionary(
            kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);

    /// <summary>DI ve yapılandırma seçenekleri ile fabrika örneği oluşturur.</summary>
    public PaymentGatewayFactory(IServiceProvider serviceProvider, IOptionsMonitor<TriPayOptions> options)
    {
        _serviceProvider = serviceProvider;
        _options = options;
    }

    /// <summary>Gateway kodu için provider örneğini senkron döndürür (destek kontrolü yapmaz).</summary>
    public IPaymentGatewayProvider? GetProvider(string? gatewayName = null)
    {
        var name = string.IsNullOrWhiteSpace(gatewayName) ? PaymentGatewayNames.Default : gatewayName;
        return _providers.TryGetValue(name, out var providerType)
            ? _serviceProvider.GetService(providerType) as IPaymentGatewayProvider
            : null;
    }

    /// <summary>Provider'ın kayıtlı, aktif ve yapılandırmada desteklendiğini doğrulayarak döndürür.</summary>
    public async Task<IPaymentGatewayProvider?> GetGatewayProviderAsync(string? gatewayName = null)
    {
        var provider = GetProvider(gatewayName);
        if (provider == null || !provider.IsSystemActive)
            return null;

        return await provider.IsSupportedAsync() ? provider : null;
    }

    /// <summary><see cref="TriPayOptions.DefaultGateway"/> veya VakıfPayS varsayılanı ile aktif provider döndürür.</summary>
    public async Task<IPaymentGatewayProvider?> GetActiveGatewayProviderAsync()
    {
        var defaultName = _options.CurrentValue.DefaultGateway;
        return await GetGatewayProviderAsync(
            string.IsNullOrWhiteSpace(defaultName) ? PaymentGatewayNames.VakifPays : defaultName);
    }

    /// <summary>Kodda kayıtlı tüm gateway kodlarını listeler.</summary>
    public IReadOnlyList<string> GetAllAvailableGateways()
    {
        return _providers.Keys.ToList();
    }

    /// <summary>Sistemde aktif işaretli provider gateway adlarını listeler.</summary>
    public IReadOnlyList<string> GetSystemActiveGatewayNames()
    {
        return _providers
            .Select(x => GetProvider(x.Key))
            .Where(x => x is { IsSystemActive: true })
            .Select(x => x!.GatewayName)
            .ToList();
    }
}
