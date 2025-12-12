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
    }
}