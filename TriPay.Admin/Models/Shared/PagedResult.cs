namespace TriPay.Admin.Models.Shared;

/// <summary>Sayfalanmış liste sonucu.</summary>
public sealed class PagedResult<T> : PagedResultBase
{
    public required IReadOnlyList<T> Items { get; init; }
}
