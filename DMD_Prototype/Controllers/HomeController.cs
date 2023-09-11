using DMD_Prototype.Data;
using DMD_Prototype.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DMD_Prototype.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _Db;
        private readonly List<MTIModel> _MTIModels;

        public HomeController(AppDbContext _context)
        {
           _Db = _context;
            _MTIModels = _Db.MTIDb.ToList();
        }

        public IActionResult Index()
        {

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult LogoutOptions(string option)
        {
            switch (option)
            {
                case "Admin":
                    {
                        return RedirectToAction("AdminView", "Admin");
                    }
                case "Logout":
                    {
                        return RedirectToAction("LoginPage", "Login");
                    }
                default:
                    {
                        return RedirectToAction("AdminView", "Admin");
                    }
            }
        }

        public IActionResult MTIList(string whichDoc)
        {
            TempData["Subj"] = whichDoc;
            return View(_MTIModels.Where(j => j.Product == whichDoc));
        }
    }
}