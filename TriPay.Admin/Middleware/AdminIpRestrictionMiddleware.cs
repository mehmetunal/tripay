using System.Net;

namespace TriPay.Admin.Middleware;

/// <summary>İsteğe bağlı admin panel IP kısıtı (<c>TriPay:Admin:AllowedIpRanges</c>).</summary>
public sealed class AdminIpRestrictionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IReadOnlyList<string> _allowed;

    public AdminIpRestrictionMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _allowed = configuration.GetSection("TriPay:Admin:AllowedIpRanges").Get<string[]>() ?? [];
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (_allowed.Count > 0 && !IsAllowed(context.Connection.RemoteIpAddress))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("IP adresiniz yönetim paneline erişim için yetkili değil.");
            return;
        }

        await _next(context);
    }

    private bool IsAllowed(IPAddress? remote)
    {
        if (remote == null)
            return false;

        var ip = remote.MapToIPv4().ToString();
        return _allowed.Any(range => ip.StartsWith(range.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}
