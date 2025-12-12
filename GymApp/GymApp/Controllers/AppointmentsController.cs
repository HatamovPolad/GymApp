using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims; // Kullanıcı ID'sini bulmak için gerekli
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization; // Yetkilendirme için gerekli
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GymApp.Data;
using GymApp.Models;

namespace GymApp.Controllers
{
    [Authorize] // Sadece giriş yapmış kullanıcılar erişebilir
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. LİSTELEME (INDEX)
        // Admin hepsini görür, Üye sadece kendininkini görür.
        public async Task<IActionResult> Index()
        {
            var gymContext = _context.Appointments
                .Include(a => a.GymService)
                .Include(a => a.Trainer)
                .Include(a => a.User)
                .AsQueryable();

            // Eğer Admin DEĞİLSE, sadece kendi randevularını filtrele
            if (!User.IsInRole("Admin"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                gymContext = gymContext.Where(a => a.UserId == userId);
            }

            return View(await gymContext.ToListAsync());
        }

        // 2. DETAY (DETAILS)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.GymService)
                .Include(a => a.Trainer)
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appointment == null) return NotFound();

            // GÜVENLİK: Üye başkasının randevusuna bakamasın (Admin hariç)
            if (!User.IsInRole("Admin"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (appointment.UserId != userId) return Forbid(); // Erişim Yasak
            }

            return View(appointment);
        }

        // 3. OLUŞTURMA (CREATE) - GET
        public IActionResult Create()
        {
            // Dropdown'larda ID yerine İSİM gözüksün:
            ViewData["TrainerId"] = new SelectList(_context.Trainers, "Id", "FullName");
            ViewData["GymServiceId"] = new SelectList(_context.GymServices, "ServiceId", "ServiceName");

            return View();
        }

        // 3. OLUŞTURMA (CREATE) - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Date,TrainerId,GymServiceId")] Appointment appointment)
        {
            // 1. Giriş yapan kullanıcının ID'sini al
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            appointment.UserId = userId;

            // 2. Otomatik değerleri ata
            appointment.CreatedDate = DateTime.Now;
            appointment.Status = "Bekliyor";

            // 3. Tarih kontrolü
            if (appointment.Date <= DateTime.Now)
            {
                ModelState.AddModelError("Date", "Geçmiş bir tarihe randevu alamazsınız.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(appointment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Hata varsa listeleri tekrar doldur
            ViewData["TrainerId"] = new SelectList(_context.Trainers, "Id", "FullName", appointment.TrainerId);
            ViewData["GymServiceId"] = new SelectList(_context.GymServices, "ServiceId", "ServiceName", appointment.GymServiceId);

            return View(appointment);
        }

        // ==========================================
        // DÜZENLEME (EDIT) - SADECE ADMIN
        // ==========================================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            ViewData["GymServiceId"] = new SelectList(_context.GymServices, "ServiceId", "ServiceName", appointment.GymServiceId);
            ViewData["TrainerId"] = new SelectList(_context.Trainers, "Id", "FullName", appointment.TrainerId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName", appointment.UserId);

            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Date,Status,CreatedDate,UserId,TrainerId,GymServiceId")] Appointment appointment)
        {
            if (id != appointment.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(appointment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AppointmentExists(appointment.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["GymServiceId"] = new SelectList(_context.GymServices, "ServiceId", "ServiceName", appointment.GymServiceId);
            ViewData["TrainerId"] = new SelectList(_context.Trainers, "Id", "FullName", appointment.TrainerId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName", appointment.UserId);
            return View(appointment);
        }

        // ==========================================
        // SİLME / İPTAL (DELETE) - ADMIN VE SAHİBİ
        // ==========================================

        // GET: Delete Sayfası
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.GymService)
                .Include(a => a.Trainer)
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appointment == null) return NotFound();

            // GÜVENLİK: Eğer Admin değilse VE randevu kendisinin değilse SİLEMEZ
            if (!User.IsInRole("Admin"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (appointment.UserId != userId) return Forbid(); // Yasak
            }

            return View(appointment);
        }

        // POST: Delete İşlemi
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                // GÜVENLİK KONTROLÜ (Tekrar)
                if (!User.IsInRole("Admin"))
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    // Başkasının randevusunu silmeye çalışıyorsa durdur
                    if (appointment.UserId != userId) return Forbid();
                }

                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.Id == id);
        }
    }
}