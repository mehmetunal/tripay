namespace TriPay.Data.Repositories.Admin;

/// <summary>Admin listeleri için sayfalı sonuç.</summary>
public sealed class AdminPagedResult<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}
