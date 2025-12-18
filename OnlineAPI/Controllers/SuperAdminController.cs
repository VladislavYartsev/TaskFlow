using Microsoft.AspNetCore.Mvc;

namespace OnlineAPI.Controllers
{
    public class SuperAdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
