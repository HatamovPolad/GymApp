using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
namespace GymApp.Controllers
{
    public class AccountController : Controller
    {
        // Identity yöneticilerini tanımlıyoruz
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        // Constructor'da bunları içeri alıyoruz (Dependency Injection)
        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // 1. KAYIT OLMA SAYFASI (GET)
        public IActionResult Register()
        {
            return View();
        }

        // 2. KAYIT OLMA İŞLEMİ (POST)
        // Not: User modelini sildiğimiz için parametreleri tek tek alıyoruz.
        [HttpPost]
        public async Task<IActionResult> Register(string fullName, string email, string password)
        {
            if (ModelState.IsValid)
            {
                // Yeni bir Identity kullanıcısı oluştur
                var user = new IdentityUser
                {
                    // Basit yaklaşım: Kullanıcı adı (UserName) fullName olsun, email ayrı tutulur
                    UserName = string.IsNullOrWhiteSpace(fullName) ? email : fullName,
                    Email = email,
                    EmailConfirmed = true // Şimdilik onayı geçiyoruz
                };

                // Kullanıcıyı oluştur (Şifreyi hashleyerek kaydeder)
                var result = await _userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    // Varsayılan olarak "Member" rolünü ver
                    await _userManager.AddToRoleAsync(user, "Member");

                    // Login'e yönlendirelim
                    return RedirectToAction("Login");
                }

                // Hata varsa (örn: şifre yetersiz, email kayıtlı) ekrana bas
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View();
        }

        // 3. GİRİŞ SAYFASI (GET)
        public IActionResult Login()
        {
            return View();
        }

        // 4. GİRİŞ İŞLEMİ (POST)
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            // Kullanıcıyı bul
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                // Şifre kontrolü ve giriş yapma işlemi
                // false, false parametreleri: "Beni hatırla" kapalı, "Kilitleme" kapalı
                var result = await _signInManager.PasswordSignInAsync(user, password, false, false);

                if (result.Succeeded)
                {
                    // Başarılıysa anasayfaya git
                    return RedirectToAction("Index", "Home");
                }
            }

            // Hata varsa
            ViewBag.Error = "E-posta veya şifre hatalı!";
            return View();
        }

        // 5. ÇIKIŞ YAP
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        // 6. YETKİSİZ GİRİŞ SAYFASI (Erişim Engellendi)
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}