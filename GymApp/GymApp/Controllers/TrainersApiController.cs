using Microsoft.AspNetCore.Mvc;
using GymApp.Data;
using GymApp.Models;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainersApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TrainersApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/TrainersApi
        // Tüm antrenörleri JSON formatında getirir
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetTrainers()
        {
            // "Select" kullanarak sadece istediğimiz verileri alıyoruz.
            // Bu sayede "Döngüsel Başvuru" (Circular Reference) hatasından kurtuluruz.
            var trainers = await _context.Trainers
                .Select(t => new
                {
                    Id = t.Id,
                    AdSoyad = t.FullName,
                    Uzmanlik = t.Specialization
                })
                .ToListAsync();

            return Ok(trainers);
        }
    }
}