using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Bu kütüphane gerekli

namespace GymApp.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Lütfen bir tarih seçiniz.")]
        [Display(Name = "Randevu Tarihi")]
        public DateTime Date { get; set; }

        [Display(Name = "Durum")]
        public string Status { get; set; } = "Bekliyor"; // Bekliyor, Onaylandı, Reddedildi

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // --- İLİŞKİLER ---

        // 1. ÜYE (IdentityUser)
        [Display(Name = "Üye")]
        public string? UserId { get; set; }
        [ForeignKey("UserId")]
        public IdentityUser? User { get; set; }

        // 2. ANTRENÖR (Trainer)
        [Display(Name = "Antrenör")]
        [Required(ErrorMessage = "Lütfen antrenör seçiniz.")]
        public int TrainerId { get; set; }

        [ForeignKey("TrainerId")]
        public Trainer? Trainer { get; set; }

        // 3. HİZMET (GymService)
        [Display(Name = "Hizmet")]
        [Required(ErrorMessage = "Lütfen hizmet seçiniz.")]
        public int GymServiceId { get; set; }

        // DİKKAT: GymService tablosunun anahtarı 'ServiceId' olduğu için bunu belirtmemiz gerekebilir
        // Ama EF Core genelde GymServiceId ismini ServiceId ile eşleştiremeyebilir.
        // En garantisi ForeignKey attribute'u kullanmak.
        [ForeignKey("GymServiceId")]
        public GymService? GymService { get; set; }
    }
}