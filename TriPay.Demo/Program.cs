// TriPay demo web uygulaması giriş noktası: MVC, ödeme servisleri ve yönlendirme boru hattını yapılandırır.
using Microsoft.EntityFrameworkCore;
using TriPay.Core.Redis;
using TriPay.Data.DependencyInjection;
using TriPay.Data.Persistence;
using TriPay.Persistence.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddTriPayHosted(builder.Configuration);

var app = builder.Build();
app.Services.RunTriPayMigrations();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapGet("/health/live", () => Results.Ok(new { status = "live", utc = DateTime.UtcNow }));

app.MapGet("/health/ready", async (ITriPayRedisCache redis, TriPayDbContext db, CancellationToken ct) =>
{
    var redisOk = await redis.PingAsync(ct);
    var dbOk = await db.Database.CanConnectAsync(ct);

    if (redisOk && dbOk)
        return Results.Ok(new { status = "ready", redis = true, database = true, utc = DateTime.UtcNow });

    return Results.Json(
        new { status = "degraded", redis = redisOk, database = dbOk, utc = DateTime.UtcNow },
        statusCode: StatusCodes.Status503ServiceUnavailable);
});

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
