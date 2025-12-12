using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymApp.Controllers
{
    // Sadece 'Admin' rolü olanlar buraya girebilir.
    // Eğer üye girerse "Access Denied" sayfasına atılır.
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}