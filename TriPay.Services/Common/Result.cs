namespace TriPay.Services.Common;

public class Result<T>
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public T? Data { get; init; }

    public static Result<T> Success(T data) => new()
    {
        IsSuccess = true,
        Data = data
    };

    public static Result<T> Failure(string errorMessage) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage
    };
}
