using InsureYouAıNew.Context;
using InsureYouAıNew.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace InsureYouAı.Controllers
{
    public class ServiceController : Controller
    {
        private readonly InsureContext _context;
        private readonly IConfiguration _configuration;
        public ServiceController(InsureContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;

        }
        public IActionResult ServiceList()
        {
            var values = _context.Services.ToList();

            return View(values);
        }
        [HttpGet]
        public IActionResult CreateService()
        {
            return View();
        }
        [HttpPost]
        public IActionResult CreateService(Service service)
        {
            _context.Services.Add(service);
            _context.SaveChanges();
            return RedirectToAction("ServiceList");
        }

        [HttpGet]
        public IActionResult UpdateService(int id)
        {
            var value = _context.Services.Find(id);
            return View(value);
        }
        [HttpPost]
        public IActionResult UpdateService(Service service)
        {
            _context.Services.Update(service);
            _context.SaveChanges();
            return RedirectToAction("ServiceList");
        }

        public IActionResult DeleteService(int id)
        {
            var value = _context.Services.Find(id);
            _context.Services.Remove(value);
            _context.SaveChanges();
            return RedirectToAction("ServiceList");
        }
        [HttpGet]
        public async Task<IActionResult> CreateServiceGoogleGemini(int id)
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
                                text = "Kurumsal bir sigorta firması için hizmetler (services) başlık ve açıklamalarını oluştur. Her biri kısa başlık + kısa açıklama olsun. En az 6 adet hizmet üret."
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
            var serviceText = jsonDoc.RootElement
                                 .GetProperty("candidates")[0]
                                 .GetProperty("content")
                                 .GetProperty("parts")[0]
                                 .GetProperty("text")
                                 .GetString();

            var services = serviceText?
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            ViewBag.services = services;
            return View();
        }
    }
}
