// TriPay demo: Framework modu (AddTriPayFramework) — ödeme verisi üye işyeri uygulamasında kalır.
using TriPay.Core.Redis;
using TriPay.Demo.Services;
using TriPay.Persistence.DependencyInjection;
using TriPay.Services.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddTriPayFramework(builder.Configuration);
builder.Services.AddSingleton<IDemoOrderStore, InMemoryDemoOrderStore>();
builder.Services.AddSingleton<CheckoutGatewayInfoService>();
builder.Services.AddSingleton<DemoPaymentDiagnosticStore>();
builder.Services.AddScoped<FrameworkDemoPaymentService>();

var app = builder.Build();

var diagnosticStore = app.Services.GetRequiredService<DemoPaymentDiagnosticStore>();
PaymentDiagnostic.Enabled = true;
PaymentDiagnostic.RegisterSink(diagnosticStore);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapGet("/health/live", () => Results.Ok(new { status = "live", mode = "framework", utc = DateTime.UtcNow }));

app.MapGet("/health/ready", async (ITriPayRedisCache redis, CancellationToken ct) =>
{
    var redisOk = await redis.PingAsync(ct);
    if (redisOk)
        return Results.Ok(new { status = "ready", mode = "framework", redis = true, utc = DateTime.UtcNow });

    return Results.Json(
        new { status = "degraded", mode = "framework", redis = redisOk, utc = DateTime.UtcNow },
        statusCode: StatusCodes.Status503ServiceUnavailable);
});

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Checkout}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
