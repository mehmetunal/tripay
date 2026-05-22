namespace TriPay.Admin.Models.Shared;

/// <summary>Sayfalama meta verisi (view partial için).</summary>
public class PagedResultBase
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public int TotalCount { get; init; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}
