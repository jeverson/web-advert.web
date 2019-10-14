using Microsoft.AspNetCore.Mvc;

namespace WebAdvert.Web.Controllers
{
    public class AdvertManagement : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}