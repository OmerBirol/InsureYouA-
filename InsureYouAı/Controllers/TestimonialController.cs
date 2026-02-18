using InsureYouAı.Entities;
using InsureYouAıNew.Context;
using InsureYouAıNew.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace InsureYouAı.Controllers
{
    public class TestimonialController : Controller
    {
        private readonly InsureContext _context;
        private readonly IConfiguration _configuration;
        public TestimonialController(InsureContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;

        }
        public IActionResult TestimonialList()
        {
            var values = _context.Testimonials.ToList();

            return View(values);
        }
        [HttpGet]
        public IActionResult CreateTestimonial()
        {
            return View();
        }
        [HttpPost]
        public IActionResult CreateTestimonial(Testimonial testimonial)
        {
            _context.Testimonials.Add(testimonial);
            _context.SaveChanges();
            return RedirectToAction("TestimonialList");
        }

        [HttpGet]
        public IActionResult UpdateTestimonial(int id)
        {
            var value = _context.Testimonials.Find(id);
            return View(value);
        }
        [HttpPost]
        public IActionResult UpdateTestimonial(Testimonial testimonial)
        {
            _context.Testimonials.Update(testimonial);
            _context.SaveChanges();
            return RedirectToAction("TestimonialList");
        }

        public IActionResult DeleteTestimonial(int id)
        {
            var value = _context.Testimonials.Find(id);
            _context.Testimonials.Remove(value);
            _context.SaveChanges();
            return RedirectToAction("TestimonialList");
        }

        [HttpGet]
        public async Task<IActionResult> CreateTestimonialGoogleGemini(int id)
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
                                text = "Sigorta firması için müşteri yorumları oluştur. Her satır tek bir yorum olsun. Format: Ad Soyad - Unvan - Yorum. En az 6 yorum üret."
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
            var testimonialText = jsonDoc.RootElement
                                 .GetProperty("candidates")[0]
                                 .GetProperty("content")
                                 .GetProperty("parts")[0]
                                 .GetProperty("text")
                                 .GetString();

            var testimonials = testimonialText?
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            ViewBag.testimonials = testimonials;
            return View();
        }
    }
}
