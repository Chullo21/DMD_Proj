using Microsoft.AspNetCore.Mvc;

namespace DMD_Prototype.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult LoginPage()
        {
            return View();
        }

        public IActionResult LoginAcc(string user, string pass)
        {
            return RedirectToAction("Index", "Home");
        }
    }
}
