using Microsoft.Extensions.DependencyInjection;
using TriPay.Services.PaymentGateways.Interfaces;
using TriPay.Services.PaymentGateways.Providers;

namespace TriPay.Services.PaymentGateways;

public class PaymentGatewayFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, Type> _providers = new(StringComparer.OrdinalIgnoreCase)
    {
        ["VakifPays"] = typeof(VakifPaysGatewayProvider)
    };

    public PaymentGatewayFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IPaymentGatewayProvider? GetProvider(string? gatewayName = null)
    {
        var name = string.IsNullOrWhiteSpace(gatewayName) ? "VakifPays" : gatewayName;
        return _providers.TryGetValue(name, out var providerType)
            ? _serviceProvider.GetService(providerType) as IPaymentGatewayProvider
            : null;
    }

    public async Task<IPaymentGatewayProvider?> GetGatewayProviderAsync(string? gatewayName = null)
    {
        var provider = GetProvider(gatewayName);
        if (provider == null || !provider.IsSystemActive)
            return null;

        return await provider.IsSupportedAsync() ? provider : null;
    }

    public async Task<IPaymentGatewayProvider?> GetActiveGatewayProviderAsync()
    {
        return await GetGatewayProviderAsync("VakifPays");
    }

    public IReadOnlyList<string> GetAllAvailableGateways()
    {
        return _providers.Keys.ToList();
    }

    public IReadOnlyList<string> GetSystemActiveGatewayNames()
    {
        return _providers
            .Select(x => GetProvider(x.Key))
            .Where(x => x is { IsSystemActive: true })
            .Select(x => x!.GatewayName)
            .ToList();
    }
}
