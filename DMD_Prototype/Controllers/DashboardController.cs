using Microsoft.AspNetCore.Mvc;

namespace DMD_Prototype.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
