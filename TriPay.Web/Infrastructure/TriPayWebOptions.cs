namespace TriPay.Web.Infrastructure;

public sealed class TriPayWebOptions
{
    public const string SectionName = "TriPayWeb";

    public string PayDemoUrl { get; set; } = "https://localhost:7293";
    public string AdminPanelUrl { get; set; } = "https://localhost:5055";
    public string ContactEmail { get; set; } = "iletisim@tripay.com.tr";
}
