using System.ComponentModel.DataAnnotations;

namespace GymApp.Models
{
    public class GymService
    {
        [Key]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Hizmet adı zorunludur.")]
        [Display(Name = "Hizmet Adı")]
        public string ServiceName { get; set; }

        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        // SQL'de Duration değil, DurationMinutes olduğu için değiştirdik:
        [Required(ErrorMessage = "Süre girmek zorunludur.")]
        [Display(Name = "Süre (Dakika)")]
        [Range(1, 480, ErrorMessage = "Süre en az 1 dakika olmalıdır.")]
        public int DurationMinutes { get; set; }

        [Required(ErrorMessage = "Ücret girmek zorunludur.")]
        [Display(Name = "Ücret (TL)")]
        [Range(0, 100000, ErrorMessage = "Ücret 0'dan küçük olamaz.")]
        public decimal Price { get; set; }
    }
}