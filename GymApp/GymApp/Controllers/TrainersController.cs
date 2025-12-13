using Microsoft.AspNetCore.Authorization; // Yetkilendirme için şart
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymApp.Data;
using GymApp.Models;

namespace GymApp.Controllers
{
    public class TrainersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrainersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. LİSTELEME: Herkes görebilir
        public async Task<IActionResult> Index()
        {
            return View(await _context.Trainers.ToListAsync());
        }

        // 2. DETAY: Herkes görebilir
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var trainer = await _context.Trainers
                .FirstOrDefaultAsync(m => m.Id == id); // TrainerId yerine Id kullandık

            if (trainer == null) return NotFound();

            return View(trainer);
        }

        // ==========================================
        // YÖNETİCİ İŞLEMLERİ (SADECE ADMIN)
        // ==========================================

        // 3. EKLEME (CREATE)
        [Authorize(Roles = "Admin")] // KİLİT
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,FullName,Specialization,WorkStartTime,WorkEndTime")] Trainer trainer)
        {
            // --- KURAL 1: SALON SAATLERİ KONTROLÜ (09:00 - 23:00) ---
            TimeSpan salonAcilis = new TimeSpan(9, 0, 0);  // 09:00
            TimeSpan salonKapanis = new TimeSpan(23, 0, 0); // 23:00

            if (trainer.WorkStartTime < salonAcilis || trainer.WorkEndTime > salonKapanis)
            {
                ModelState.AddModelError("WorkStartTime", "Hocanın mesai saatleri salonun çalışma saatleri (09:00 - 23:00) dışında olamaz!");
            }

            // --- KURAL 2: MANTIK HATASI KONTROLÜ ---
            // Başlangıç saati bitişten sonra olamaz
            if (trainer.WorkStartTime >= trainer.WorkEndTime)
            {
                ModelState.AddModelError("WorkEndTime", "Mesai bitiş saati, başlangıç saatinden önce olamaz.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(trainer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(trainer);
        }

        // 4. DÜZENLEME (EDIT)
        [Authorize(Roles = "Admin")] // KİLİT
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var trainer = await _context.Trainers.FindAsync(id);
            if (trainer == null) return NotFound();
            return View(trainer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,Specialization,WorkStartTime,WorkEndTime")] Trainer trainer)
        {
            if (id != trainer.Id) return NotFound();

            // --- KURAL 1: SALON SAATLERİ KONTROLÜ ---
            TimeSpan salonAcilis = new TimeSpan(9, 0, 0);
            TimeSpan salonKapanis = new TimeSpan(23, 0, 0);

            if (trainer.WorkStartTime < salonAcilis || trainer.WorkEndTime > salonKapanis)
            {
                ModelState.AddModelError("WorkStartTime", "Hocanın mesai saatleri salonun çalışma saatleri (09:00 - 23:00) dışında olamaz!");
            }

            // --- KURAL 2: MANTIK HATASI KONTROLÜ ---
            if (trainer.WorkStartTime >= trainer.WorkEndTime)
            {
                ModelState.AddModelError("WorkEndTime", "Mesai bitiş saati, başlangıç saatinden önce olamaz.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(trainer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TrainerExists(trainer.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(trainer);
        }

        // 5. SİLME (DELETE)
        [Authorize(Roles = "Admin")] // KİLİT
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var trainer = await _context.Trainers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (trainer == null) return NotFound();

            return View(trainer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // KİLİT
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trainer = await _context.Trainers.FindAsync(id);
            if (trainer != null)
            {
                _context.Trainers.Remove(trainer);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TrainerExists(int id)
        {
            return _context.Trainers.Any(e => e.Id == id);
        }
    }
}