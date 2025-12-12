using Microsoft.EntityFrameworkCore;
using GymApp.Data;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// 1. Veritabanı Bağlantısı
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Bağlantı adresi bulunamadı.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Identity (Üyelik) Sistemini Ekleme
// Şifre "sau" olabilsin diye kuralları gevşetiyoruz.
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 3; // "sau" 3 harfli
    options.User.RequireUniqueEmail = true;
    // Kullanıcı adında boşluk, Türkçe karakter vb. her şeye izin ver:
    options.User.AllowedUserNameCharacters = null;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 3. Çerez Ayarları (Giriş yaptıktan sonraki ayarlar)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// --- OTOMATİK ADMİN EKLEME (SEED) KODU ---
// Proje her açıldığında Admin var mı diye bakar, yoksa ekler.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    // Birazdan oluşturacağımız SeedData sınıfını çağırıyoruz
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

app.UseAuthentication(); // Kimlik Doğrulama (Kimsin?)
app.UseAuthorization();  // Yetkilendirme (Yetkin var mı?)

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();