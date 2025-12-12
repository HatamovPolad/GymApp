using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // Bu kütüphane ŞART
using Microsoft.EntityFrameworkCore;
using GymApp.Models;

namespace GymApp.Data
{
    // DÜZELTME: DbContext yerine IdentityDbContext kullanıyoruz.
    // Bu sayede Users, Roles gibi tablolar otomatik geliyor.
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        //Identity sistemi kullanıcıları kendi içinde "AspNetUsers" tablosunda tutar.
        public DbSet<Trainer> Trainers { get; set; }
        public DbSet<GymService> GymServices { get; set; } // PDF'teki Hizmetler (GymService)
        public DbSet<Appointment> Appointments { get; set; } // PDF'teki Randevular
    }
}