using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymApp.Data;
using GymApp.Models;

namespace GymApp.Controllers
{
    public class GymServicesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GymServicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. LİSTELEME
        public async Task<IActionResult> Index()
        {
            return View(await _context.GymServices.ToListAsync());
        }

        // 2. DETAY
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var gymService = await _context.GymServices
                .FirstOrDefaultAsync(m => m.ServiceId == id); // DEĞİŞTİ: Id -> ServiceId

            if (gymService == null) return NotFound();

            return View(gymService);
        }

        // --- ADMIN İŞLEMLERİ ---

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        // DEĞİŞTİ: Bind içine ServiceId ve DurationMinutes yazdık
        public async Task<IActionResult> Create([Bind("ServiceId,ServiceName,Description,DurationMinutes,Price")] GymService gymService)
        {
            if (ModelState.IsValid)
            {
                _context.Add(gymService);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(gymService);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var gymService = await _context.GymServices.FindAsync(id);
            if (gymService == null) return NotFound();
            return View(gymService);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("ServiceId,ServiceName,Description,DurationMinutes,Price")] GymService gymService)
        {
            if (id != gymService.ServiceId) return NotFound(); // DEĞİŞTİ: Id -> ServiceId

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(gymService);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GymServiceExists(gymService.ServiceId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(gymService);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var gymService = await _context.GymServices
                .FirstOrDefaultAsync(m => m.ServiceId == id); // DEĞİŞTİ: Id -> ServiceId

            if (gymService == null) return NotFound();

            return View(gymService);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // FindAsync varsayılan olarak Primary Key (ServiceId) arar
            var gymService = await _context.GymServices.FindAsync(id);
            if (gymService != null)
            {
                _context.GymServices.Remove(gymService);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool GymServiceExists(int id)
        {
            return _context.GymServices.Any(e => e.ServiceId == id); // DEĞİŞTİ
        }
    }
}