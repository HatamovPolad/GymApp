using Microsoft.EntityFrameworkCore;
using GymApp.Data;
using Microsoft.AspNetCore.Authentication.Cookies; // Basit giriş için gerekli

var builder = WebApplication.CreateBuilder(args);

// 1. Veritabanı Bağlantısı
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Bağlantı adresi bulunamadı.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Basit Çerez (Cookie) Bazlı Giriş Sistemi
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Giriş yapmamış kişiyi buraya at
        options.AccessDeniedPath = "/Account/AccessDenied"; // Yetkisi yetmeyeni buraya at
    });

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Hata Ayıklama
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Önce kimlik doğrulama, sonra yetkilendirme
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();