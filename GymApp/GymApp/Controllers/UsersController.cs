using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Controllers
{
    [Authorize(Roles = "Admin")] // Sadece Admin girebilir
    public class UsersController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public UsersController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        // Üyeleri Listele
        public async Task<IActionResult> Index()
        {
            // 1. Şu an sisteme giriş yapmış olan kişinin (Admin'in) ID'sini bul
            var currentUserId = _userManager.GetUserId(User);

            // 2. Veritabanından kullanıcıları çekerken filtrele:
            // "ID'si benim ID'me eşit OLMAYANLARI (u.Id != currentUserId) getir"
            var users = await _userManager.Users
                .Where(u => u.Id != currentUserId)
                .ToListAsync();

            return View(users);
        }

        // Üye Silme Sayfası (Onay için)
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        // Üye Silme İşlemi (Kesin Sil)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}