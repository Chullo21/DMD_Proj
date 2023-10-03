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
        private readonly List<StartWorkModel> _swModel;
        private readonly List<AccountModel> _users;
        private readonly List<ProblemLogModel> _problems;

        public HomeController(AppDbContext _context)
        {
            _Db = _context;
            _MTIModels = _Db.MTIDb.ToList();
            _swModel = _Db.StartWorkDb.ToList();
            _users = _Db.AccountDb.Where(j => j.Role == "USER").ToList();
            _problems = _Db.PLDb.ToList();
        }

        public IActionResult Index()
        {
            return View(DashboardDetGetter());
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

        public IActionResult MTIList(string whichDoc, string? whichType)
        {
            string type = whichType ?? "MPI";

            TempData["Subj"] = whichDoc;
            TempData["DocType"] = type;
            return View(_MTIModels.Where(j => j.Product == whichDoc && j.DocType == type));
        }

        public IActionResult ShowTravelers()
        {
            List<StartWorkModel> models = new List<StartWorkModel>();

            foreach (var sw in _swModel)
            {
                sw.UserID = _users.FirstOrDefault(j => j.UserID == sw.UserID).AccName;

                models.Add(sw);
            }

            return View(models.OrderByDescending(j => j.StartDate));
        }

        private IndexModel DashboardDetGetter()
        {
            IndexModel mod = new IndexModel();
            {
                mod.ControlledVal =  _MTIModels.Count();
                mod.InterimVal = _problems.Count(j => j.PLSDStatus == "OPEN");
                mod.ObsoleteVal = _MTIModels.Count(j => j.ObsoleteStat);
                mod.JTPVal = _problems.Count(j => j.Product == "JTP");
                mod.JLPVal = _problems.Count(j => j.Product == "JLP");
                mod.OLBVal = _problems.Count(j => j.Product == "OLB");
                mod.PNPVal = _problems.Count(j => j.Product == "PNP");
            }

            return mod;
        }
    }

    public class IndexModel
    {
        public int ControlledVal { get; set; }
        public int InterimVal { get; set; }
        public int ObsoleteVal { get; set; }

        public int JTPVal { get; set; }
        public int JLPVal { get; set; }
        public int PNPVal { get; set; }
        public int OLBVal { get; set; }
    }
}
