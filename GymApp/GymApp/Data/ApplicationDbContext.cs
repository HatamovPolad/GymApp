using Microsoft.EntityFrameworkCore;
using GymApp.Models;

namespace GymApp.Data
{
    // Artık IdentityDbContext değil, sadece DbContext kullanıyoruz
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } // Bizim yeni kullanıcı tablomuz
        public DbSet<Trainer> Trainers { get; set; }
        public DbSet<GymService> GymServices { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
    }
}