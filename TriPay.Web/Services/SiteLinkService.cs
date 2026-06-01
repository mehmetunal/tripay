using Microsoft.Extensions.Options;
using TriPay.Web.Infrastructure;

namespace TriPay.Web.Services;

public sealed class SiteLinkService(IOptions<TriPayWebOptions> options) : ISiteLinkService
{
    private readonly TriPayWebOptions _opts = options.Value;

    public string PayDemoUrl => _opts.PayDemoUrl;
    public string AdminPanelUrl => _opts.AdminPanelUrl;
    public string ContactEmail => _opts.ContactEmail;
    public string CompanyName => _opts.CompanyName;
    public string CompanyUrl => _opts.CompanyUrl;

    public string Root => "/";
    public string DocsPath => "/docs";
    public string PayPath => "/pay";
    public string AdminPath => "/admin";
}
