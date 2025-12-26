using Microsoft.AspNetCore.Mvc;

namespace WebCrudApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
