using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Bu kütüphane gerekli

namespace GymApp.Models
{
    public class GymService
    {
        [Key]
        public int ServiceId { get; set; }

        [Required]
        [Display(Name = "Hizmet Adı")]
        public string ServiceName { get; set; }

        [Display(Name = "Süre (Dakika)")]
        public int DurationMinutes { get; set; }

        [Display(Name = "Ücret")]
        [Column(TypeName = "decimal(18,2)")] // BU SATIRI EKLEDİK: Toplam 18 basamak, 2'si virgülden sonra (kuruş)
        public decimal Price { get; set; }

        // İlişkiler
        public ICollection<Appointment>? Appointments { get; set; }
    }
}