using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Newtonsoft.Json;

namespace GymApp.Controllers
{
    [Authorize]
    public class AiController : Controller
    {
        // 🔑 1. ADIMDA ALDIĞIN ANAHTARI AŞAĞIYA YAPIŞTIR:
        private const string ApiKey = "AIzaSyCKR4PEovZq8ez9fbJmYzEPNc14I1o1Ivs";

        // Google Gemini API Adresi
        // "gemini-2.5-flash" modelini kullanıyoruz. Hem çok hızlı hem de çok zeki.
        private const string ApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key="; public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GeneratePlan(int age, int weight, int height, string goal, string gender)
        {
            // 1. Kullanıcı verilerini View'dan alıp tekrar geri göndermek için sakla (Form silinmesin diye)
            ViewBag.Age = age;
            ViewBag.Weight = weight;
            ViewBag.Height = height;

            // 2. Yapay Zekaya gönderilecek soruyu (Prompt) hazırla
            string userPrompt = $"Ben {age} yaşında, {weight} kilo, {height} cm boyunda, {gender} cinsiyetinde bir bireyim. " +
                                $"Hedefim: {goal}. " +
                                $"Bana profesyonel bir spor hocası ve diyetisyen gibi davran. " +
                                $"1. Bölüm: Haftalık antrenman programı (gün gün). " +
                                $"2. Bölüm: Örnek günlük beslenme programı (kalori hesaplı). " +
                                $"Cevabı Türkçe ver, samimi ve motive edici bir dil kullan. " +
                                $"Format olarak Markdown kullan (Kalın başlıklar, maddeler).";

            // 3. Google Gemini'nin istediği JSON formatını hazırla
            var requestBody = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = userPrompt } } }
                }
            };

            string jsonContent = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                using (var httpClient = new HttpClient())
                {
                    // API'ye isteği gönder
                    var response = await httpClient.PostAsync(ApiUrl + ApiKey, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var resultJson = await response.Content.ReadAsStringAsync();
                        dynamic result = JsonConvert.DeserializeObject(resultJson);

                        // Gelen cevabın içinden metni cımbızla çek
                        string aiResponse = result.candidates[0].content.parts[0].text;

                        ViewBag.AiResponse = aiResponse;
                    }
                    else
                    {
                        // Google'dan gelen gerçek hata mesajını oku
                        var errorResponse = await response.Content.ReadAsStringAsync();
                        ViewBag.Error = $"Google API Hatası! Kodu: {response.StatusCode}. Detay: {errorResponse}";
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Bir hata oluştu: " + ex.Message;
            }

            return View("Index");
        }
    }
}