using Microsoft.AspNetCore.Mvc;

namespace DMD_Prototype.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult AdminView()
        {
            return View();
        }

        public IActionResult AccountsView()
        {
            return View();
        }
    }
}
