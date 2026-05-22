namespace TriPay.Core.Gateways;

/// <summary>Gateway URL, durum kodu ve hata mesajı metadata'sı (DB + Redis önbellek).</summary>
public interface IGatewayMetadataService
{
    /// <summary>Tek ayar değeri döner (test/prod ortamına göre).</summary>
    Task<string?> GetSettingAsync(string gatewayCode, string settingKey, bool isTestMode, CancellationToken cancellationToken = default);

    /// <summary>Provider hata kodunu kullanıcı mesajına çevirir.</summary>
    Task<string?> GetErrorMessageAsync(string gatewayCode, string? providerErrorCode, string locale = "tr", CancellationToken cancellationToken = default);

    /// <summary>Tüm ayarları sözlük olarak döner (provider başlatma).</summary>
    Task<IReadOnlyDictionary<string, string>> GetSettingsAsync(string gatewayCode, bool isTestMode, CancellationToken cancellationToken = default);

    /// <summary>Tüm hata eşlemelerini sözlük olarak döner (senkron normalize için önbellek).</summary>
    Task<IReadOnlyDictionary<string, string>> GetErrorMapAsync(string gatewayCode, string locale = "tr", CancellationToken cancellationToken = default);
}
