using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using SporSalonuMVC.Models; // Projendeki namespace

var builder = WebApplication.CreateBuilder(args);

// Veritabaný baðlantýsý (PostgreSQL)
builder.Services.AddDbContext<SporSalonuDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Oturum (Session) ve MVC servisleri
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddControllersWithViews();

// Türkçe kültür ayarlarý
var cultureInfo = new CultureInfo("tr-TR");
cultureInfo.NumberFormat.CurrencyDecimalSeparator = ",";
cultureInfo.NumberFormat.NumberDecimalSeparator = ",";

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(cultureInfo),
    SupportedCultures = new List<CultureInfo> { cultureInfo },
    SupportedUICultures = new List<CultureInfo> { cultureInfo }
};

var app = builder.Build();

// Middleware sýrasý önemli
app.UseRequestLocalization(localizationOptions);
app.UseStaticFiles();
app.UseRouting();
app.UseSession(); // Mutlaka routing'den sonra

// ?? ÖZEL ROUTE BURAYA
app.MapControllerRoute(
    name: "entry",
    pattern: "giriscikisekrani",
    defaults: new { controller = "Entry", action = "Index" }
);

// ?? DÝÐER TÜM STANDART ROUTE'LAR
app.MapDefaultControllerRoute();

app.Run();
