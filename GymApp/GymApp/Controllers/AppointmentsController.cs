using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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

        // ==========================================
        // 1. LİSTELEME (INDEX)
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var gymContext = _context.Appointments
                .Include(a => a.GymService)
                .Include(a => a.Trainer)
                .Include(a => a.User)
                .AsQueryable();

            // Eğer Admin DEĞİLSE, sadece kendi randevularını görsün
            if (!User.IsInRole("Admin"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                gymContext = gymContext.Where(a => a.UserId == userId);
            }

            // Tarihe göre sıralayalım (En yakın tarih en üstte)
            return View(await gymContext.OrderByDescending(a => a.Date).ToListAsync());
        }

        // ==========================================
        // 2. DETAY (DETAILS)
        // ==========================================
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
                if (appointment.UserId != userId) return Forbid();
            }

            return View(appointment);
        }

        // ==========================================
        // 3. OLUŞTURMA (CREATE) & AJAX İŞLEMLERİ
        // ==========================================

        // GET: Create
        public IActionResult Create()
        {
            // Hizmetleri dolduruyoruz
            ViewData["GymServiceId"] = new SelectList(_context.GymServices, "ServiceId", "ServiceName");

            // Antrenör listesini BOŞ gönderiyoruz. Kullanıcı hizmet seçince JS dolduracak.
            ViewData["TrainerId"] = new SelectList(Enumerable.Empty<SelectListItem>(), "Id", "FullName");

            return View();
        }

        // ★ AJAX 1: Hizmete Göre Hocaları Getir
        [HttpGet]
        public JsonResult GetTrainersByService(int serviceId)
        {
            var trainers = _context.TrainerServices
                .Where(ts => ts.GymServiceId == serviceId)
                .Select(ts => new
                {
                    id = ts.Trainer.Id,
                    fullName = ts.Trainer.FullName
                })
                .ToList();

            return Json(trainers);
        }

        // ★ AJAX 2: Hizmetin Fiyat ve Süresini Getir (YENİ EKLENDİ)
        [HttpGet]
        public async Task<JsonResult> GetServiceDetails(int serviceId)
        {
            var service = await _context.GymServices.FindAsync(serviceId);

            if (service == null) return Json(null);

            // Fiyatı ve süreyi JSON olarak döndür
            return Json(new { price = service.Price, duration = service.Sure });
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Date,TrainerId,GymServiceId")] Appointment appointment)
        {
            // 1. Kullanıcıyı Bul ve Ata
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            appointment.UserId = userId;
            appointment.CreatedDate = DateTime.Now;
            appointment.Status = "Bekliyor";

            // 2. Geçmiş Tarih Kontrolü
            if (appointment.Date <= DateTime.Now)
            {
                ModelState.AddModelError("Date", "Geçmiş bir tarihe randevu alamazsınız.");
            }

            // --- İŞ MANTIĞI VE KONTROLLER ---
            var selectedService = await _context.GymServices.FindAsync(appointment.GymServiceId);
            var selectedTrainer = await _context.Trainers.FindAsync(appointment.TrainerId);

            if (selectedService != null && selectedTrainer != null)
            {
                // Başlangıç ve Bitiş Saatlerini Hesapla
                DateTime startTime = appointment.Date;
                DateTime endTime = startTime.AddMinutes(selectedService.Sure);

                // SALON KURALLARI (Sabit: 09:00 - 23:00)
                TimeSpan salonAcilis = new TimeSpan(9, 0, 0);
                TimeSpan salonKapanis = new TimeSpan(23, 0, 0);

                // KURAL 1: GÜN DEĞİŞİMİ VE SALON KAPANIŞ KONTROLÜ
                if (endTime.Date > startTime.Date || endTime.TimeOfDay > salonKapanis || startTime.TimeOfDay < salonAcilis)
                {
                    ModelState.AddModelError("Date",
                        $"Salonumuz 09:00 - 23:00 arası hizmet vermektedir. Seçtiğiniz hizmet ({selectedService.ServiceName}) {selectedService.Sure} dakika sürüyor ve kapanış saatini aşıyor.");
                }

                // KURAL 2: ANTRENÖR MESAİSİ KONTROLÜ
                if (startTime.TimeOfDay < selectedTrainer.WorkStartTime || endTime.TimeOfDay > selectedTrainer.WorkEndTime)
                {
                    ModelState.AddModelError("Date",
                        $"Antrenör {selectedTrainer.FullName} belirtilen saatlerde çalışmıyor. (Mesai: {selectedTrainer.WorkStartTime:hh\\:mm} - {selectedTrainer.WorkEndTime:hh\\:mm}).");
                }

                // KURAL 3: HOCA ÇAKIŞMA KONTROLÜ
                bool isTrainerBusy = await _context.Appointments.AnyAsync(a =>
                    a.TrainerId == appointment.TrainerId &&
                    a.Status != "İptal Edildi" &&
                    (a.Date < endTime && a.Date.AddMinutes(a.GymService.Sure) > startTime)
                );

                if (isTrainerBusy)
                {
                    ModelState.AddModelError("", "Seçtiğiniz antrenörün bu saat aralığında başka bir randevusu var.");
                }

                // KURAL 4: ÜYE ÇAKIŞMA KONTROLÜ
                bool isUserBusy = await _context.Appointments.AnyAsync(a =>
                    a.UserId == userId &&
                    a.Status != "İptal Edildi" &&
                    (a.Date < endTime && a.Date.AddMinutes(a.GymService.Sure) > startTime)
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

            // Hata olursa sayfayı tekrar yükle
            ViewData["TrainerId"] = new SelectList(_context.Trainers, "Id", "FullName", appointment.TrainerId);
            ViewData["GymServiceId"] = new SelectList(_context.GymServices, "ServiceId", "ServiceName", appointment.GymServiceId);
            return View(appointment);
        }

        // ==========================================
        // 4. DÜZENLEME (EDIT) - SADECE ADMIN
        // ==========================================

        // GET: Edit
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            ViewData["GymServiceId"] = new SelectList(_context.GymServices, "ServiceId", "ServiceName", appointment.GymServiceId);
            ViewData["TrainerId"] = new SelectList(_context.Trainers, "Id", "FullName", appointment.TrainerId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", appointment.UserId);

            return View(appointment);
        }

        // POST: Edit
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
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", appointment.UserId);
            return View(appointment);
        }

        // ==========================================
        // 5. SİLME / İPTAL (DELETE)
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
                    if (appointment.UserId != userId) return Forbid();
                }

                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 6. ONAYLAMA VE İPTAL (SADECE ADMIN)
        // ==========================================

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            appointment.Status = "Onaylandı"; // Durumu güncelle
            _context.Update(appointment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Cancel(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            appointment.Status = "İptal Edildi"; // Durumu güncelle
            _context.Update(appointment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.Id == id);
        }
    }
}