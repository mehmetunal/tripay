namespace TriPay.Web.Services;

public interface ISiteLinkService
{
    string PayDemoUrl { get; }
    string AdminPanelUrl { get; }
    string ContactEmail { get; }
    string CompanyName { get; }
    string CompanyUrl { get; }
    /// <summary>Kök (kurumsal) — <c>/</c></summary>
    string Root { get; }
    string DocsPath { get; }
    string PayPath { get; }
    string AdminPath { get; }
}
