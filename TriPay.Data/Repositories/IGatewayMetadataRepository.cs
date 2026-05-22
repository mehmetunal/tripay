using TriPay.Data.Entities;

namespace TriPay.Data.Repositories;

/// <summary>Gateway ayar ve hata eşleme veri erişimi.</summary>
public interface IGatewayMetadataRepository
{
    /// <summary>Kanal koduna göre gateway kimliğini döner.</summary>
    Task<int?> GetPaymentGatewayIdByCodeAsync(string gatewayCode, CancellationToken cancellationToken = default);

    /// <summary>Aktif gateway ayarlarını getirir.</summary>
    Task<IReadOnlyList<GatewaySetting>> GetSettingsAsync(int paymentGatewayId, CancellationToken cancellationToken = default);

    /// <summary>Aktif hata eşlemelerini getirir.</summary>
    Task<IReadOnlyList<GatewayErrorMapping>> GetErrorMappingsAsync(int paymentGatewayId, string locale, CancellationToken cancellationToken = default);
}
