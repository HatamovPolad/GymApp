using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymApp.Controllers
{
    [Authorize] // Sadece giriş yapan üyeler kullanabilsin
    public class AiController : Controller
    {
        // 1. FORM SAYFASI (GET)
        public IActionResult Index()
        {
            return View();
        }

        // 2. SONUÇ ÜRETME (POST)
        [HttpPost]
        public IActionResult GeneratePlan(int age, int weight, int height, string goal)
        {
            string plan = "";
            string diet = "";
            string status = ""; // Zayıf, Normal, Kilolu

            // 1. BMI Hesapla: Kilo / (Boy * Boy) [Boy metre cinsinden]
            // Boyu cm'den metreye çeviriyoruz (Örn: 175 -> 1.75)
            double heightInMeters = height / 100.0;

            // Sıfıra bölünme hatasını önleyelim
            if (heightInMeters <= 0) heightInMeters = 1.70;

            double bmi = weight / (heightInMeters * heightInMeters);

            // 2. Vücut Analizi (BMI Değerine Göre)
            if (bmi < 18.5) status = "Zayıf";
            else if (bmi < 25) status = "Normal Kilolu";
            else if (bmi < 30) status = "Fazla Kilolu";
            else status = "Obezite Sınırında";

            // 3. Yaş Kontrolü (Gençler için uyarı)
            bool isYoung = age < 18;

            // --- YAPAY ZEKA KARAR MEKANİZMASI ---

            // HEDEF: KİLO VERMEK
            if (goal == "lose_weight")
            {
                if (bmi < 20) // Zaten zayıfsa kilo vermeyi önerme
                {
                    plan = "Dikkat: Vücut kitle indeksin düşük. Kilo vermen sağlık açısından riskli olabilir! Bunun yerine kas kütleni artırarak sıkılaşmaya odaklanmalısın.";
                    diet = "Kaliteli karbonhidrat (pilav, makarna) ve protein alımını artırarak sağlıklı kilo almaya çalış.";
                }
                else
                {
                    plan = "Haftada 3 gün HIIT (Yüksek Yoğunluklu) Kardiyo + 2 gün Tüm Vücut Ağırlık Antrenmanı.";
                    diet = "Günlük kalori ihtiyacından 300-500 kalori eksik al. Akşam 8'den sonra karbonhidratı kes. Şekeri hayatından çıkar.";
                }
            }
            // HEDEF: KAS YAPMAK
            else if (goal == "build_muscle")
            {
                plan = "Haftada 4 veya 5 gün Bölgesel Antrenman (Push/Pull/Legs sistemi önerilir). Ağır kilolarla az tekrar (8-12) çalış.";
                diet = "Kilonun 2 katı kadar (gram) protein al (Tavuk, Balık, Yumurta). Antrenman sonrası mutlaka karbonhidrat tüket.";
            }
            // HEDEF: FORM KORUMAK
            else
            {
                plan = "Haftada 3 gün tüm vücut (Full Body) antrenmanı. Aralarda 30 dk hafif tempolu yürüyüş.";
                diet = "Dengeli Akdeniz diyeti uygula. Sebze ağırlıklı beslen ve işlenmiş gıdalardan uzak dur.";
            }

            // YAŞ VE GELİŞİM NOTU
            if (isYoung)
            {
                plan += " (Not: Gelişim çağında olduğun için omurgana çok yük bindiren hareketlerden kaçın, kendi vücut ağırlığınla çalışman daha güvenli.)";
            }
            else if (age > 50)
            {
                plan += " (Önemli: Antrenmanlardan önce 15 dakika ısınma ve esneme hareketlerini sakın ihmal etme.)";
            }

            // Sonuçları View'a (Görünüme) gönder
            ViewBag.Plan = plan;
            ViewBag.Diet = diet;
            // F1: Virgülden sonra 1 basamak göster demek (Örn: 24.5)
            ViewBag.Status = $"Vücut İndeksin: {bmi:F1} ({status})";

            // Form sayfasına geri dön ama verilerle birlikte
            return View("Index");
        }
    }
}