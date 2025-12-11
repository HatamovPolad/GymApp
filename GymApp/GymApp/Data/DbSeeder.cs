using Microsoft.AspNetCore.Identity;
// Constants satırını sildik, gerek yok.

namespace GymApp.Data  // DİKKAT: Burası GymApp.Data oldu
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider service)
        {
            // Kullanıcı ve Rol yöneticilerini çağırıyoruz
            var userManager = service.GetService<UserManager<IdentityUser>>();
            var roleManager = service.GetService<RoleManager<IdentityRole>>();

            // 1. ROLLERİ OLUŞTUR (Admin ve Member)
            // Eğer roller zaten varsa tekrar oluşturma hatası vermesin diye kontrol etmiyoruz, 
            // CreateAsync zaten varsa işlem yapmaz.
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            if (!await roleManager.RoleExistsAsync("Member"))
                await roleManager.CreateAsync(new IdentityRole("Member"));

            // 2. DEFAULT ADMIN OLUŞTUR
            var adminEmail = "b231210559@sakarya.edu.tr"; // Öğrenci Numaran

            var user = await userManager.FindByEmailAsync(adminEmail);
            if (user == null)
            {
                user = new IdentityUser()
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                };

                var result = await userManager.CreateAsync(user, "sau");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }
        }
    }
}