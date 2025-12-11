using System.ComponentModel.DataAnnotations;

namespace GymApp.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad Soyad zorunludur.")]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "E-Posta zorunludur.")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } // Şifreyi burada saklayacağız

        public string Role { get; set; } = "Member"; // Varsayılan rol "Member" (Üye) olsun
    }
}