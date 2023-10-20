using DMD_Prototype.Data;
using DMD_Prototype.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;

namespace DMD_Prototype.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _Db;
        private readonly ISharedFunct ishare;

        public HomeController(AppDbContext _context, ISharedFunct _ishared)
        {
            _Db = _context;
            ishare = _ishared;
        }

        public ContentResult SearchDocument(string searchString)
        {
            MTIModel model = ishare.GetMTIs().FirstOrDefault(j => j.DocumentNumber == searchString || j.AssemblyPN == searchString || j.AssemblyDesc == searchString);
            string res = "";

            if (model == null)
            {
                res = JsonConvert.SerializeObject(new { failed = "f" });
            }
            else
            {
                res = JsonConvert.SerializeObject(new { failed = "p", documentNumber = model.DocumentNumber.ToString() });
            }

            //return RedirectToAction("MTIView", "MTI", new {docuNumber = model.DocumentNumber, workStat = false, sesID = ""});
            return Content(res, "application/json");
        }

        public ContentResult GetOrigName(string userId)
        {           
            return Content(JsonConvert.SerializeObject(new {Name = ishare.GetAccounts().FirstOrDefault(j => j.UserID == userId).AccName }), "application/json");
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

            MTIListModel list = new MTIListModel();
            {
                list.list = ishare.GetMTIs().Where(j => j.Product == whichDoc && j.DocType == type).OrderByDescending(j => j.DateCreated).ToList();
                list.Originators = ishare.GetAccounts().Where(j => j.Role == "ORIGINATOR").Select(j => j.AccName).ToList();
            }

            return View(list);
        }

        public IActionResult ShowTravelers()
        {
            List<StartWorkModel> models = new List<StartWorkModel>();

            foreach (var sw in ishare.GetStartWork())
            {
                sw.UserID = ishare.GetAccounts().FirstOrDefault(j => j.UserID == sw.UserID).AccName;
                models.Add(sw);
            }

            TravelerViewModel model = new TravelerViewModel();
            {
                model.Traveler = models;
                model.Users = ishare.GetAccounts().Where(j => j.Role == "USER").Select(j => j.AccName).ToList();
            }

            return View(model);
        }

        private int[] DataPerMonthGetter(List<ProblemLogModel> list)
        {
            int[] res = new int[DateTime.Now.Month];

            foreach (var log in list)
            {
                res[log.LogDate.Month - 1]++;
            }

            return res;
        }

        private IndexModel DashboardDetGetter()
        {
            List<MTIModel> mtis = ishare.GetMTIs();
            List<ProblemLogModel> pls = ishare.GetProblemLogs();

            IndexModel mod = new IndexModel();
            {
                mod.ControlledVal =  mtis.Count();               
                mod.ObsoleteVal = mtis.Count(j => j.ObsoleteStat);
                mod.JTPVal = pls.Count(j => j.Product == "JTP");
                mod.JLPVal = pls.Count(j => j.Product == "JLP");
                mod.OLBVal = pls.Count(j => j.Product == "OLB");
                mod.PNPVal = pls.Count(j => j.Product == "PNP");

                mod.InterimVal = pls.Count(j => j.PLIDStatus == "OPEN");

                mod.OpenPL = JsonConvert.SerializeObject(DataPerMonthGetter(pls.Where(j => j.PLSDStatus == "OPEN" || string.IsNullOrEmpty(j.PLSDStatus)).OrderBy(j => j.LogDate).ToList()));
                mod.ClosedPL = JsonConvert.SerializeObject(DataPerMonthGetter(pls.Where(j => j.PLSDStatus == "CLOSED").OrderBy(j => j.LogDate).ToList()));
            }

            return mod;
        }
    }

    public class IndexModel
    {
        public int ControlledVal { get; set; }
        public int InterimVal { get; set; }
        public string OpenPL { get; set; }
        public string ClosedPL { get; set; }
        public int ObsoleteVal { get; set; }

        public int JTPVal { get; set; }
        public int JLPVal { get; set; }
        public int PNPVal { get; set; }
        public int OLBVal { get; set; }
    }

    public class MTIListModel
    {
        public List<MTIModel>? list { get; set;}
        public List<string>? Originators { get; set; }
    }

    public class TravelerViewModel
    {
        public List<StartWorkModel> Traveler { get; set; }
        public List<string> Users { get; set; }
    }
}
