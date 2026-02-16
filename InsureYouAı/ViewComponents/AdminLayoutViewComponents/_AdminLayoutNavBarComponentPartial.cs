using Microsoft.AspNetCore.Mvc;

namespace InsureYouAı.ViewComponents.AdminLayoutViewComponents
{
    public class _AdminLayoutNavBarComponentPartial:ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
