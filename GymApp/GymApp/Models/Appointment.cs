using System.ComponentModel.DataAnnotations;

namespace GymApp.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        [Required]
        [Display(Name = "Randevu Tarihi")]
        public DateTime AppointmentDate { get; set; }

        public bool IsConfirmed { get; set; } = false;

        // İlişkiler
        [Display(Name = "Antrenör")]
        public int TrainerId { get; set; }
        public Trainer Trainer { get; set; }

        [Display(Name = "Hizmet")]
        public int ServiceId { get; set; }
        public GymService GymService { get; set; }

        // Üye ID'si (Identity sistemi bağlanınca dolacak)
        public string? MemberId { get; set; }
    }
}