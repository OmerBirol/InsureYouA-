using Microsoft.AspNetCore.Mvc;

namespace InsureYouAı.Controllers
{
    public class AdminLayoutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
