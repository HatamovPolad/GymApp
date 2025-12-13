using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymApp.Controllers
{
    [Authorize]
    public class AiController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult GeneratePlan(int age, int weight, int height, string goal)
        {
            string plan = "";
            string diet = "";
            string status = "";

            double heightInMeters = height / 100.0;
            if (heightInMeters <= 0) heightInMeters = 1.70;

            double bmi = weight / (heightInMeters * heightInMeters);

            if (bmi < 18.5) status = "Zayıf";
            else if (bmi < 25) status = "Normal Kilolu";
            else if (bmi < 30) status = "Fazla Kilolu";
            else status = "Obezite Sınırında";

            bool isYoung = age < 18;

            if (goal == "lose_weight")
            {
                if (bmi < 20)
                {
                    plan = "Vücut kitle indeksin düşük, kilo vermen önerilmez. Kas kazanımına odaklan.";
                    diet = "Protein ve karbonhidrat ağırlıklı beslen.";
                }
                else
                {
                    plan = "Haftada 3 gün Kardiyo + 2 gün Ağırlık.";
                    diet = "Kalori açığı oluştur. Şekeri kes.";
                }
            }
            else if (goal == "build_muscle")
            {
                plan = "Push/Pull/Legs antrenman sistemi.";
                diet = "Yüksek protein (2g/kg).";
            }
            else
            {
                plan = "Full Body antrenman + Yürüyüş.";
                diet = "Dengeli Akdeniz diyeti.";
            }

            if (isYoung) plan += " (Kendi vücut ağırlığınla çalışman önerilir.)";

            ViewBag.Plan = plan;
            ViewBag.Diet = diet;
            ViewBag.Status = $"BMI: {bmi:F1} ({status})";

            return View("Index");
        }
    }
}