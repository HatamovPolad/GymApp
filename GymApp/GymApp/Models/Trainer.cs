using System.ComponentModel.DataAnnotations;

namespace GymApp.Models
{
    public class Trainer
    {
        public int Id { get; set; } // TrainerId DEĞİL, sadece Id olmalı

        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; }

        [Display(Name = "Uzmanlık Alanı")]
        public string Specialization { get; set; } // Expertise DEĞİL, Specialization olmalı
        public TimeSpan WorkStartTime { get; set; } // Mesai Başlangıç (Örn: 09:00)
        public TimeSpan WorkEndTime { get; set; }   // Mesai Bitiş (Örn: 17:00)
        public ICollection<TrainerService> TrainerServices { get; set; }
    }
}