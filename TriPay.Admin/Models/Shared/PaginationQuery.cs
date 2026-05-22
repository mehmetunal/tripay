namespace TriPay.Admin.Models.Shared;

/// <summary>Sayfalama sorgu parametreleri.</summary>
public class PaginationQuery
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public int GetSkip() => Math.Max(0, (Math.Max(1, Page) - 1) * Math.Clamp(PageSize, 5, 100));
}
