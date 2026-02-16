using InsureYouAı.Entities;
using InsureYouAıNew.Context;
using InsureYouAıNew.Entities;
using Microsoft.AspNetCore.Mvc;

namespace InsureYouAı.Controllers
{
    public class TestimonialController : Controller
    {
        private readonly InsureContext _context;
        public TestimonialController(InsureContext context)
        {
            _context = context;

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
    }
}
