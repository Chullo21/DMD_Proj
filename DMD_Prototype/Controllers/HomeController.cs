using DMD_Prototype.Data;
using DMD_Prototype.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;

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
                case "Accounts":
                    {
                        return RedirectToAction("AccountsView", "Admin");
                    }
                case "Logout":
                    {
                        return RedirectToAction("LoginPage", "Login");
                    }
                default:
                    {
                        return RedirectToAction("LoginPage", "Login");
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
            Dictionary<string, (string, string)> mtis = ishare.GetMTIs().Where(j => !j.ObsoleteStat).ToDictionary(j => j.DocumentNumber, j => (j.AssemblyDesc, j.AfterTravLog));
            Dictionary<string, (string, string)> module = ishare.GetModules().ToDictionary(j => j.SessionID, j => (j.Module, j.SerialNo));
            Dictionary<string, (string, string?, string?, string, string)> sw = ishare.GetStartWork().ToDictionary(j => j.SessionID, j => (j.StartDate.ToShortDateString(), j.FinishDate.HasValue ? j.FinishDate.Value.ToShortDateString() : "NF", j.UserID, j.DocNo, j.SWID.ToString()));
            Dictionary<string, string> accs = ishare.GetAccounts().Where(j => j.Role == "USER").ToDictionary(j => j.UserID, j => j.AccName);

            TravelerViewModel res = new();
            List<TravDets> dets = new();

            foreach (var work in sw)
            {
                string stat = "";
                if (work.Value.Item2 != "NF") stat = "Done"; else if (work.Value.Item3 == null) stat = "Pending"; else stat = "On-Going";

                TravDets trav = new();
                trav.Desc = mtis.FirstOrDefault(j => j.Key == work.Value.Item4).Value.Item1;
                trav.DocNo = work.Value.Item4;
                trav.StartDate = work.Value.Item1;
                trav.FinishDate = work.Value.Item2;
                trav.Status = stat;
                trav.Technician = accs.FirstOrDefault(j => j.Key == work.Value.Item3).Value;
                trav.SerialNo = module.FirstOrDefault(j => j.Key == work.Key).Value.Item2;
                trav.Module = module.FirstOrDefault(j => j.Key == work.Key).Value.Item1;
                trav.SessionID = work.Key;
                trav.SWID = work.Value.Item5;
                trav.LogType = mtis.FirstOrDefault(j => j.Key == work.Value.Item4).Value.Item2;

                dets.Add(trav);
            }

            res.Travs = dets;
            res.Users = ishare.GetAccounts().Where(j => j.Role == "USER").Select(j => j.AccName).ToList();

            return View(res);
        }

        public ContentResult GetAllDocuments()

        {
            List<MTIModel> docs = ishare.GetMTIs().Where(j => !j.ObsoleteStat).ToList();

            return Content(JsonConvert.SerializeObject(new {r = docs}), "application/json");
        }

        public ContentResult GetRSCount()
        {
            int rsCount = ishare.GetRS().Count();

            return Content(JsonConvert.SerializeObject(new {r = rsCount}), "application/json");
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
            IEnumerable<MTIModel> mtis = ishare.GetMTIs();
            IEnumerable<ProblemLogModel> pls = ishare.GetProblemLogs();

            IndexModel mod = new IndexModel();
            {
                mod.ControlledVal =  mtis.Count();               
                mod.ObsoleteVal = mtis.Count(j => j.ObsoleteStat);
                mod.JTPVal = pls.Count(j => j.Product == "JTP" && j.Validation == "Valid");
                mod.JLPVal = pls.Count(j => j.Product == "JLP" && j.Validation == "Valid");
                mod.OLBVal = pls.Count(j => j.Product == "OLB" && j.Validation == "Valid");
                mod.PNPVal = pls.Count(j => j.Product == "PNP" && j.Validation == "Valid");

                mod.InterimVal = mtis.Count(j => j.MTPIStatus == 'i');

                mod.OpenIDPL = JsonConvert.SerializeObject(DataPerMonthGetter(pls.Where(j => j.PLIDStatus == "OPEN" && j.Validation == "Valid").OrderBy(j => j.LogDate).ToList()));
                mod.ClosedIDPL = JsonConvert.SerializeObject(DataPerMonthGetter(pls.Where(j => j.PLIDStatus == "CLOSED" && j.Validation == "Valid").OrderBy(j => j.LogDate).ToList()));

                mod.OpenSDPL = JsonConvert.SerializeObject(DataPerMonthGetter(pls.Where(j => j.PLSDStatus == "OPEN" && j.Validation == "Valid").OrderBy(j => j.LogDate).ToList()));
                mod.ClosedSDPL = JsonConvert.SerializeObject(DataPerMonthGetter(pls.Where(j => j.PLSDStatus == "CLOSED" && j.Validation == "Valid").OrderBy(j => j.LogDate).ToList()));

                mod.PNPCount = mtis.Count(j => j.Product == "PNP" && !j.ObsoleteStat);
                mod.JLPCount = mtis.Count(j => j.Product == "JLP" && !j.ObsoleteStat);
                mod.JTPCount = mtis.Count(j => j.Product == "JTP" && !j.ObsoleteStat);
                mod.OLBCount = mtis.Count(j => j.Product == "OLB" && !j.ObsoleteStat);
                mod.SWAPCount = mtis.Count(j => j.Product == "SWAP" && !j.ObsoleteStat);
                mod.SPARESCount = mtis.Count(j => j.Product == "SPARES" && !j.ObsoleteStat);

                mod.AllDocs = mtis.Where(j => !j.ObsoleteStat).ToList();               
            }

            return mod;
        }
    }

    public class IndexModel
    {
        public int ControlledVal { get; set; }
        public int InterimVal { get; set; }
        public string OpenIDPL { get; set; }
        public string ClosedIDPL { get; set; }
        public string OpenSDPL { get; set; }
        public string ClosedSDPL { get; set; }
        public int ObsoleteVal { get; set; }

        public int JTPVal { get; set; }
        public int JLPVal { get; set; }
        public int PNPVal { get; set; }
        public int OLBVal { get; set; }

        public int PNPCount { get; set; }
        public int JLPCount { get; set; }
        public int JTPCount { get; set; }
        public int OLBCount { get; set; }
        public int SWAPCount { get; set; }
        public int SPARESCount { get; set; }

        public IEnumerable<MTIModel>? AllDocs { get; set; }
    }

    public class MTIListModel
    {
        public IEnumerable<MTIModel>? list { get; set;}
        public IEnumerable<string>? Originators { get; set; }
    }

    public class TravDets
    {
        public string Desc { get; set; } = string.Empty;
        public string DocNo { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string FinishDate { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Technician { get; set; } = string.Empty;
        public string SerialNo { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public string SessionID { get; set; } = string.Empty;
        public string LogType { get; set; } = string.Empty;
        public string SWID { get; set; } = string.Empty;
    }

    public class TravelerViewModel
    {
        public IEnumerable<TravDets> Travs { get; set; }

        public IEnumerable<string> Users { get; set; }
    }
}
