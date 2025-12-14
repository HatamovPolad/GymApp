using System.Diagnostics;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace GymApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // KURAL: Eğer giren kişi Admin ise, direkt Yönetim Paneline şutla! 🚀
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("AdminPanel");
            }

            // Değilse normal anasayfayı görsün
            return View();
        }

        // Admin Paneli Sayfasını Açan Kod (Eğer Controller'da yoksa bunu da ekle)
        [Authorize(Roles = "Admin")]
        public IActionResult AdminPanel()
        {
            return View();
        }

        // Privacy Action'ı silebilirsin veya durabilir, ama View'dan linki kaldıracağız.
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
