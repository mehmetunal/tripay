namespace TriPay.Core.Gateways;

/// <summary>Gateway ayarlarını (appsettings TriPay:Gateways veya ileride veritabanı) sağlar.</summary>
public interface IGatewaySettingsProvider
{
    /// <summary>Belirtilen gateway kodu için etkin yapılandırmayı döndürür; yoksa veya kapalıysa null.</summary>
    /// <param name="gatewayName"><see cref="PaymentGatewayNames"/> sabitlerinden biri.</param>
    /// <param name="cancellationToken">İptal belirteci.</param>
    Task<GatewayConfig?> GetGatewayConfigAsync(string gatewayName, CancellationToken cancellationToken = default);
}
