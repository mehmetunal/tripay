using TriPay.Admin.Extensions;
using TriPay.Admin.Middleware;
using TriPay.Admin.Services;
using TriPay.Data.DependencyInjection;
using TriPay.Data.Identity;
using TriPay.Data.Persistence;
using TriPay.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddTriPayAdminFluentValidation();
builder.Services.AddAntiforgery(options => options.HeaderName = "RequestVerificationToken");
builder.Services.AddTriPayData(builder.Configuration);
builder.Services.AddTriPayInfrastructure(builder.Configuration);
builder.Services.AddTriPayIdentity();
builder.Services.AddTriPayAdminAuthorization();
builder.Services.AddTriPayAdminApplication();
builder.Services.AddScoped<IGatewayCacheInvalidator, GatewayCacheInvalidator>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});

var app = builder.Build();

app.Services.RunTriPayMigrations();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TriPayDbContext>();
    await TriPayDbSeed.EnsureDemoDataAsync(db);
}

await AdminIdentitySeeder.SeedAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseMiddleware<AdminIpRestrictionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
