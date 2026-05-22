namespace TriPay.Core.Common;

/// <summary>İşlem sonucunu veri veya hata mesajı ile taşıyan genel sonuç sarmalayıcısıdır.</summary>
/// <typeparam name="T">Başarılı durumda dönen veri tipi.</typeparam>
public class Result<T>
{
    /// <summary>İşlemin başarılı olup olmadığını belirtir.</summary>
    public bool IsSuccess { get; init; }

    /// <summary>Başarısız işlemlerde kullanıcıya veya loga yazılacak hata metnidir.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Başarılı işlemlerde veya hata detayı için dönen veri yüküdür.</summary>
    public T? Data { get; init; }

    /// <summary>Veri ile başarılı sonuç oluşturur.</summary>
    public static Result<T> Success(T data) => new()
    {
        IsSuccess = true,
        Data = data
    };

    /// <summary>Hata mesajı ile başarısız sonuç oluşturur.</summary>
    public static Result<T> Failure(string errorMessage) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage
    };

    /// <summary>Hata mesajı ve kısmi yanıt verisi ile başarısız sonuç oluşturur.</summary>
    public static Result<T> Failure(string errorMessage, T? data) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage,
        Data = data
    };
}
