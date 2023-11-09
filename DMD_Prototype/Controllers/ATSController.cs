using Microsoft.AspNetCore.Mvc;

namespace DMD_Prototype.Controllers
{
    public class ATSController : Controller
    {
        public IActionResult ATSMenu()
        {
            return View();
        }
    }
}
