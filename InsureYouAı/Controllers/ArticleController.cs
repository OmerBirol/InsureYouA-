using InsureYouAı.Entities;
using InsureYouAıNew.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InsureYouAı.Controllers
{
    public class ArticleController : Controller
    {
        private readonly InsureContext _context;
        public ArticleController(InsureContext context)
        {
            _context = context;
        }

        public IActionResult ArticleList()
        {
            var values = _context.Articles.ToList();

            return View(values);
        }
        [HttpGet]
        public IActionResult CreateArticle()
        {
            ViewBag.Categories = new SelectList(_context.Categories.ToList(), "CategoryId", "CategoryName");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateArticle(Article article)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(_context.Categories.ToList(), "CategoryId", "CategoryName", article.CategoryId);
                return View(article);
            }

            // Seçilen kategori gerçekten var mı kontrolü
            if (!_context.Categories.Any(c => c.CategoryId == article.CategoryId))
            {
                ModelState.AddModelError(nameof(article.CategoryId), "Seçilen kategori geçersiz.");
                ViewBag.Categories = new SelectList(_context.Categories.ToList(), "CategoryId", "CategoryName", article.CategoryId);
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
    }
}
