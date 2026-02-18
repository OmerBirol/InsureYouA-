using InsureYouAıNew.Context;
using InsureYouAıNew.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace InsureYouAı.Controllers
{
    public class AboutItemController : Controller
    {
        private readonly InsureContext _context;
        private readonly IConfiguration _configuration;
        public AboutItemController(InsureContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;

        }
        public IActionResult AboutItemList()
        {
            var values = _context.AboutItems.ToList();

            return View(values);
        }
        [HttpGet]
        public IActionResult CreateAboutItem()
        {
            return View();
        }
        [HttpPost]
        public IActionResult CreateAboutItem(AboutItem aboutItem)
        {
            _context.AboutItems.Add(aboutItem);
            _context.SaveChanges();
            return RedirectToAction("AboutItemList");
        }

        [HttpGet]
        public IActionResult UpdateAboutItem(int id)
        {
            var value = _context.AboutItems.Find(id);
            return View(value);
        }
        [HttpPost]
        public IActionResult UpdateAboutItem(AboutItem aboutItem)
        {
            _context.AboutItems.Update(aboutItem);
            _context.SaveChanges();
            return RedirectToAction("AboutItemList");
        }

        public IActionResult DeleteAboutItem(int id)
        {
            var value = _context.AboutItems.Find(id);
            _context.AboutItems.Remove(value);
            _context.SaveChanges();
            return RedirectToAction("AboutItemList");
        }
        [HttpGet]
        public async Task<IActionResult> CreateAboutItemGoogleGemini(int id)
        {
            var apiKey = _configuration["Gemini:ApiKey"] ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                ViewBag.value = "Gemini API anahtarı bulunamadı.";
                return View();
            }
            var model = "gemini-2.5-flash";
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";
            
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new
                            {
                                text = "Kurumsal bir sigorta firması için etkileyici, güven verici ve profesyonel bir 'Hakkımızda alanları (about item)' yazısı oluştur. Örneğin: 'Geleceğinizi güvence altına alan kapsamlı sigorta çözümleri sunuyoruz.' şeklinde veya bunun gibi ve buna benzer daha zengin içerikler gelsin. En az 10 tane item istiyorum."
                            }
                        }
                    }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            using var httpClient = new HttpClient();
            var response = await httpClient.PostAsync(url, content);
            var responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    var modelsUrl = $"https://generativelanguage.googleapis.com/v1beta/models?key={apiKey}";
                    var modelsJson = await httpClient.GetStringAsync(modelsUrl);
                    ViewBag.value = "Model bulunamadı. Mevcut modeller: " + modelsJson;
                    return View();
                }

                ViewBag.value = "Bir hata oluştu: " + response.StatusCode + " " + responseJson;
                return View();
            }

            using var jsonDoc = JsonDocument.Parse(responseJson);
            var aboutText = jsonDoc.RootElement
                                 .GetProperty("candidates")[0]
                                 .GetProperty("content")
                                 .GetProperty("parts")[0]
                                 .GetProperty("text")
                                 .GetString();

            ViewBag.value = aboutText;
            return View();
        }
    }
}
