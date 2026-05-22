using TriPay.Web.Infrastructure;
using TriPay.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<TriPayWebOptions>(
    builder.Configuration.GetSection(TriPayWebOptions.SectionName));
builder.Services.AddSingleton<ISiteLinkService, SiteLinkService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "docs",
    pattern: "docs/{controller=Home}/{action=Index}/{id?}",
    defaults: new { area = "Docs" });

app.MapControllerRoute(
    name: "pay",
    pattern: "pay/{controller=Home}/{action=Index}/{id?}",
    defaults: new { area = "Pay" });

app.MapControllerRoute(
    name: "admin",
    pattern: "admin/{controller=Home}/{action=Index}/{id?}",
    defaults: new { area = "Admin" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
