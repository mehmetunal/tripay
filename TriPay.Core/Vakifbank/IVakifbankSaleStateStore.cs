namespace TriPay.Core.Vakifbank;

/// <summary>Vakıfbank 3D sonrası satış durumunu geçici saklayan depo sözleşmesi.</summary>
public interface IVakifbankSaleStateStore
{
    /// <summary>Satış durumunu yazar.</summary>
    Task SetAsync(string orderCode, VakifbankSaleState state, CancellationToken cancellationToken = default);

    /// <summary>Satış durumunu okur.</summary>
    Task<VakifbankSaleState?> GetAsync(string orderCode, CancellationToken cancellationToken = default);

    /// <summary>Satış durumunu siler.</summary>
    Task RemoveAsync(string orderCode, CancellationToken cancellationToken = default);
}
