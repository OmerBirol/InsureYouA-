using InsureYouAıNew.Context;
using InsureYouAıNew.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace InsureYouAı.Controllers
{
    public class AboutController : Controller
    {
        private readonly InsureContext _context;
        private readonly IConfiguration _configuration;
        public AboutController(InsureContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;

        }
        public IActionResult AboutList()
        {
            var values = _context.Abouts.ToList();

            return View(values);
        }
        [HttpGet]
        public IActionResult CreateAbout()
        {
            return View();
        }
        [HttpPost]
        public IActionResult CreateAbout(About about)
        {
            _context.Abouts.Add(about);
            _context.SaveChanges();
            return RedirectToAction("AboutList");
        }
        public IActionResult DeleteAbout(int id)
        {
            var value = _context.Abouts.Find(id);
            _context.Abouts.Remove(value);
            _context.SaveChanges();
            return RedirectToAction("AboutList");
        }

        [HttpGet]
        public IActionResult UpdateAbout(int id)
        {
            var value = _context.Abouts.Find(id);
            return View(value);
        }
        [HttpPost]
        public IActionResult UpdateAbout(About about)
        {
            _context.Abouts.Update(about);
            _context.SaveChanges();
            return RedirectToAction("AboutList");
        }
        [HttpGet]
        public async Task<IActionResult> CreateAboutGoogleGemini(int id)
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
                                text = "Kurumsal bir sigorta firması için etkileyici, güven verici ve profesyonel bir 'Hakkımızda' yazısı oluştur."
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
        //$"https://generativelanguage.googleapis.com/v1/models/{model}:generateContent?key={apiKey}";

    }
}
