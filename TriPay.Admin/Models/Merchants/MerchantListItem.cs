namespace TriPay.Admin.Models.Merchants;

public sealed class MerchantListItem
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ApiKeyMasked { get; init; } = string.Empty;
    public string? WebhookUrl { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}
