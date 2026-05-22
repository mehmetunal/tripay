namespace TriPay.Admin.Application.Dtos;

/// <summary>Sayfalı liste sonucu (presentation katmanı).</summary>
public sealed class PagedResultDto<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
}
