using DMD_Prototype.Data;
using Microsoft.AspNetCore.Mvc;

namespace DMD_Prototype.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AppDbContext _Db;
        private readonly ISharedFunct ishare;

        public DashboardController(ISharedFunct _ishare, AppDbContext db)
        {
            ishare = _ishare;
            _Db = db;
        }

        public IActionResult Get()
        {
            return View();
        }
    }
}
