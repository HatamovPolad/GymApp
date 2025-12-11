using System.ComponentModel.DataAnnotations;

namespace GymApp.Models
{
    public class Trainer
    {
        [Key]
        public int TrainerId { get; set; }

        [Required(ErrorMessage = "Ad Soyad zorunludur.")]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; }

        [Required]
        [Display(Name = "Uzmanlık Alanı")]
        public string Expertise { get; set; }

        [Display(Name = "Fotoğraf Yolu")]
        public string? ImageUrl { get; set; }

        // İlişkiler
        public ICollection<Appointment>? Appointments { get; set; }
    }
}