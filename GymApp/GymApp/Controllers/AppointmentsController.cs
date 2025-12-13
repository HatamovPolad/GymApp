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

            // Tarihe göre sıralayalım (En yakın tarih en üstte)
            return View(await gymContext.OrderByDescending(a => a.Date).ToListAsync());
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
            // 1. Kullanıcıyı Bul
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            appointment.UserId = userId;
            appointment.CreatedDate = DateTime.Now;
            appointment.Status = "Bekliyor";

            // 2. Geçmiş Tarih Kontrolü
            if (appointment.Date <= DateTime.Now)
            {
                ModelState.AddModelError("Date", "Geçmiş bir tarihe randevu alamazsınız.");
            }

            // --- KRİTİK HESAPLAMA BÖLÜMÜ ---
            var selectedService = await _context.GymServices.FindAsync(appointment.GymServiceId);
            var selectedTrainer = await _context.Trainers.FindAsync(appointment.TrainerId);

            if (selectedService != null && selectedTrainer != null)
            {
                // Başlangıç ve Bitiş Saatlerini Hesapla
                DateTime startTime = appointment.Date;
                DateTime endTime = startTime.AddMinutes(selectedService.DurationMinutes);

                // SALON KURALLARI (Sabit: 09:00 - 23:00)
                TimeSpan salonAcilis = new TimeSpan(9, 0, 0);
                TimeSpan salonKapanis = new TimeSpan(23, 0, 0);

                // KURAL 1: GÜN DEĞİŞİMİ VE SALON KAPANIŞ KONTROLÜ
                // Eğer bitiş saati başlangıç gününden farklıysa (gece yarısını geçmişse) VEYA 23:00'ü geçmişse
                if (endTime.Date > startTime.Date || endTime.TimeOfDay > salonKapanis || startTime.TimeOfDay < salonAcilis)
                {
                    ModelState.AddModelError("Date",
                        $"Salonumuz 09:00 - 23:00 arası hizmet vermektedir. Seçtiğiniz hizmet ({selectedService.ServiceName}) {selectedService.DurationMinutes} dakika sürüyor ve kapanış saatini aşıyor.");
                }

                // KURAL 2: ANTRENÖR MESAİSİ KONTROLÜ
                // Hoca o saatte çalışıyor mu? (Bitiş saatine de bakıyoruz!)
                if (startTime.TimeOfDay < selectedTrainer.WorkStartTime || endTime.TimeOfDay > selectedTrainer.WorkEndTime)
                {
                    ModelState.AddModelError("Date",
                        $"Antrenör {selectedTrainer.FullName} belirtilen saatlerde çalışmıyor. (Mesai: {selectedTrainer.WorkStartTime:hh\\:mm} - {selectedTrainer.WorkEndTime:hh\\:mm}). Randevunuz hocanın çıkış saatini aşıyor.");
                }

                // KURAL 3: HOCA ÇAKIŞMA KONTROLÜ
                bool isTrainerBusy = await _context.Appointments.AnyAsync(a =>
                    a.TrainerId == appointment.TrainerId &&
                    a.Status != "İptal Edildi" &&
                    (a.Date < endTime && a.Date.AddMinutes(a.GymService.DurationMinutes) > startTime)
                );

                if (isTrainerBusy)
                {
                    ModelState.AddModelError("", "Seçtiğiniz antrenörün bu saat aralığında başka bir randevusu var.");
                }

                // KURAL 4: ÜYE ÇAKIŞMA KONTROLÜ
                bool isUserBusy = await _context.Appointments.AnyAsync(a =>
                    a.UserId == userId &&
                    a.Status != "İptal Edildi" &&
                    (a.Date < endTime && a.Date.AddMinutes(a.GymService.DurationMinutes) > startTime)
                );

                if (isUserBusy)
                {
                    ModelState.AddModelError("", "Bu saat aralığında zaten başka bir randevunuz var.");
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(appointment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

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

        // --- ONAYLAMA VE İPTAL İŞLEMLERİ (SADECE ADMIN) ---

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            appointment.Status = "Onaylandı"; // Durumu değiştir
            await _context.SaveChangesAsync(); // Kaydet

            return RedirectToAction(nameof(Index)); // Listeye dön
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Cancel(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            appointment.Status = "İptal Edildi"; // Durumu değiştir
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.Id == id);
        }
    }
}