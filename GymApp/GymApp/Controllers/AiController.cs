using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Newtonsoft.Json;

namespace GymApp.Controllers
{
    [Authorize]
    public class AiController : Controller
    {
        private readonly IConfiguration _configuration;

        // Yapıcı Metot: Şifreleri okuyabilmek için ayarları (Configuration) içeri alıyoruz
        public AiController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Modeli buradan değiştirebilirsin
        private const string ApiModel = "gemini-2.5-flash";

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GeneratePlan(int age, int weight, int height, string goal, string gender)
        {
            // 1. GİZLİ KASADAN ŞİFREYİ OKU
            string apiKey = _configuration["GoogleApiKey"];

            if (string.IsNullOrEmpty(apiKey))
            {
                ViewBag.Error = "API Anahtarı bulunamadı! Lütfen 'Manage User Secrets' ayarını kontrol edin.";
                return View("Index");
            }

            string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{ApiModel}:generateContent?key={apiKey}";

            // View verilerini sakla
            ViewBag.Age = age;
            ViewBag.Weight = weight;
            ViewBag.Height = height;

            // Prompt Hazırla
            string userPrompt = $"Ben {age} yaşında, {weight} kilo, {height} cm boyunda, {gender} cinsiyetinde bir bireyim. " +
                                $"Hedefim: {goal}. " +
                                $"Bana profesyonel bir spor hocası gibi: " +
                                $"1. Haftalık antrenman programı. " +
                                $"2. Günlük beslenme programı (kalori hesaplı). " +
                                $"Cevabı Türkçe ver ve Markdown formatında olsun.";

            var requestBody = new
            {
                contents = new[] { new { parts = new[] { new { text = userPrompt } } } }
            };

            try
            {
                using (var httpClient = new HttpClient())
                {
                    var jsonContent = JsonConvert.SerializeObject(requestBody);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    var response = await httpClient.PostAsync(apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var resultJson = await response.Content.ReadAsStringAsync();
                        dynamic result = JsonConvert.DeserializeObject(resultJson);
                        string aiResponse = result.candidates[0].content.parts[0].text;
                        ViewBag.AiResponse = aiResponse;
                    }
                    else
                    {
                        var errorMsg = await response.Content.ReadAsStringAsync();
                        ViewBag.Error = $"Bağlantı Hatası! Detay: {errorMsg}";
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Hata: " + ex.Message;
            }

            return View("Index");
        }
    }
}