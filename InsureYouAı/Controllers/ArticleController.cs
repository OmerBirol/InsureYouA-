using InsureYouAı.Entities;
using InsureYouAıNew.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace InsureYouAı.Controllers
{
    public class ArticleController : Controller
    {
        private readonly InsureContext _context;
        private readonly IConfiguration _configuration;
        public ArticleController(InsureContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public IActionResult ArticleList()
        {
            var values = _context.Articles.ToList();

            return View(values);
        }
        [HttpGet]
        public IActionResult CreateArticle()
        {
            var categories = _context.Categories.ToList();
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");
            ViewBag.HasCategories = categories.Any();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateArticle(Article article)
        {
            if (!ModelState.IsValid)
            {
                var categories = _context.Categories.ToList();
                ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName", article.CategoryId);
                ViewBag.HasCategories = categories.Any();
                return View(article);
            }

            // Seçilen kategori gerçekten var mı kontrolü
            if (!_context.Categories.Any(c => c.CategoryId == article.CategoryId))
            {
                ModelState.AddModelError(nameof(article.CategoryId), "Seçilen kategori geçersiz.");
                var categories = _context.Categories.ToList();
                ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName", article.CategoryId);
                ViewBag.HasCategories = categories.Any();
                return View(article);
            }

            article.CreatedDate = DateTime.Now;
            _context.Articles.Add(article);
            _context.SaveChanges();
            return RedirectToAction("ArticleList");
        }

        [HttpGet]
        public IActionResult UpdateArticle(int id)
        {
            var value = _context.Articles.Find(id);
            ViewBag.Categories = new SelectList(_context.Categories.ToList(), "CategoryId", "CategoryName", value.CategoryId);
            return View(value);
        }
        [HttpPost]
        public IActionResult UpdateArticle(Article article)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(_context.Categories.ToList(), "CategoryId", "CategoryName", article.CategoryId);
                return View(article);
            }

            article.CreatedDate = DateTime.Now;
            _context.Articles.Update(article);
            _context.SaveChanges();
            return RedirectToAction("ArticleList");
        }

        public IActionResult DeleteArticle(int id)
        {
            var value = _context.Articles.Find(id);
            _context.Articles.Remove(value);
            _context.SaveChanges();
            return RedirectToAction("ArticleList");
        }
        [HttpGet]
        public IActionResult CreateArticleWithOpenAI(int id)
        {
            
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateArticleWithOpenAI(string prompt)
        {
            var apiKey = _configuration["Gemini:ApiKey"] ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                ViewBag.article = "Gemini API anahtarı bulunamadı.";
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
                                text = "Sen bir sigorta şirketi için makale yazan bir yapay zeka modelisin. Kullanıcının verdiği başlığı ve anahtar kelimeleri kullanarak sigortacılık alanında makale üret. Makale 1000 kelimelik olmalı ve anahtar kelimeleri içermelidir.\n\nKonu/anahtar kelimeler: " + prompt
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
                ViewBag.article = "Bir hata oluştu: " + response.StatusCode + " " + responseJson;
                return View();
            }

            using var jsonDoc = JsonDocument.Parse(responseJson);
            var articleText = jsonDoc.RootElement
                                 .GetProperty("candidates")[0]
                                 .GetProperty("content")
                                 .GetProperty("parts")[0]
                                 .GetProperty("text")
                                 .GetString();

            ViewBag.article = articleText;
            return View();
        }
    }
}