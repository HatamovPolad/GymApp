using Microsoft.EntityFrameworkCore;
using GymApp.Data;
using Microsoft.AspNetCore.Identity;
using System.Globalization; // Kültür ayarları için gerekli kütüphane

var builder = WebApplication.CreateBuilder(args);

// 1. Veritabanı Bağlantısı
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Bağlantı adresi bulunamadı.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Identity (Üyelik) Sistemini Ekleme
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Şifre kuralları (sau şifresine izin vermek için)
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 3;

    options.User.RequireUniqueEmail = true;

    // Kullanıcı adında her karaktere izin ver (Boşluk, Türkçe vb.)
    options.User.AllowedUserNameCharacters = null;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 3. Çerez Ayarları
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// ============================================================
// YENİ EKLENEN KISIM: TÜRKÇE VE 24 SAAT FORMATI AYARI 🕒
// ============================================================
var cultureInfo = new CultureInfo("tr-TR");
// Büyük HH = 24 saat formatı (14:30 gibi), küçük hh = 12 saat (02:30 PM gibi)
cultureInfo.DateTimeFormat.ShortTimePattern = "HH:mm";
cultureInfo.DateTimeFormat.LongTimePattern = "HH:mm:ss";

CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
// ============================================================

// --- OTOMATİK ADMİN EKLEME (SEED) KODU ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    // SeedData sınıfını çağırıp admin yoksa ekliyoruz
    await SeedData.Initialize(services);
}
// -----------------------------------------

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Kimlik Doğrulama
app.UseAuthorization();  // Yetkilendirme

var supportedCultures = new[] { new System.Globalization.CultureInfo("tr-TR") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("tr-TR"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();