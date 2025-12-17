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
        [Required(ErrorMessage = "Lütfen hizmet süresini giriniz.")]
        [Range(1, 240, ErrorMessage = "Süre en az 1 dakika olmalıdır. En fazla 4 saatlik (240 dk) bir program oluşturulabilir.")]
        public int Sure { get; set; }

        [Required(ErrorMessage = "Ücret girmek zorunludur.")]
        [Display(Name = "Ücret (TL)")]
        [Range(0, 100000, ErrorMessage = "Ücret 0'dan küçük olamaz.")]
        public decimal Price { get; set; }
        public ICollection<TrainerService> TrainerServices { get; set; }
    }
}