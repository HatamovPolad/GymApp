using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GymApp.Data;
using GymApp.Models;

namespace GymApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. KAYIT OLMA SAYFASI (GET)
        public IActionResult Register()
        {
            return View();
        }

        // 2. KAYIT OLMA İŞLEMİ (POST)
        [HttpPost]
        public IActionResult Register(User model)
        {
            if (ModelState.IsValid)
            {
                // Aynı e-posta var mı kontrol et
                if (_context.Users.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("", "Bu e-posta zaten kayıtlı.");
                    return View(model);
                }

                // Varsayılan rol: Member (Üye)
                model.Role = "Member";

                _context.Users.Add(model);
                _context.SaveChanges();

                return RedirectToAction("Login");
            }
            return View(model);
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
            // Kullanıcıyı bul (Şifre ve Email eşleşiyor mu?)
            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.Password == password);

            if (user != null)
            {
                // Kimlik bilgilerini hazırla (Çerez için)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim("FullName", user.FullName),
                    new Claim(ClaimTypes.Role, user.Role) // Rolü buraya yüklüyoruz (Admin mi Üye mi?)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties();

                // Sisteme giriş yap (Çerezi tarayıcıya gönder)
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "E-posta veya şifre hatalı!";
            return View();
        }

        // 5. ÇIKIŞ YAP
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}