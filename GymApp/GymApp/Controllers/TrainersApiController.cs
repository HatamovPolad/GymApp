using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymApp.Data;
using GymApp.Models;

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
        // Tüm antrenörleri JSON olarak döndürür (LINQ Select Kullanımı)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetTrainers()
        {
            return await _context.Trainers
                .Select(t => new {
                    AdSoyad = t.FullName,
                    Uzmanlik = t.Specialization,
                    BaslangicSaati = t.WorkStartTime.ToString(@"hh\:mm"),
                    BitisSaati = t.WorkEndTime.ToString(@"hh\:mm")
                })
                .ToListAsync();
        }
    }
}