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

        public async Task<IActionResult> ObsoleteTraveler(string SWID)
        {
            try
            {
                StartWorkModel work = (await ishare.GetStartWork()).FirstOrDefault(j => j.SWID.ToString() == SWID);
                ModuleModel module = (await ishare.GetModules()).FirstOrDefault(j => j.SessionID == work.SessionID);
                SerialNumberModel serial = (await ishare.GetSerialNumbers()).FirstOrDefault(j => j.SessionId == work.SessionID);


                if (ModelState.IsValid)
                {
                    _Db.StartWorkDb.Remove(work);
                    _Db.ModuleDb.Remove(module);
                    _Db.SerialNumberDb.Remove(serial);
                    _Db.SaveChanges();

                    Directory.Delete(Path.Combine(await ishare.GetPath("userDir"), work.SessionID), true);
                }
            }
            catch(Exception ex)
            {

            }

            return RedirectToAction("ShowTravelers", "Home");
        }

        public async Task<ContentResult> SearchDocument(string searchString)
        {
            MTIModel model = (await ishare.GetMTIs()).FirstOrDefault(j => j.DocumentNumber == searchString || j.AssemblyPN == searchString || j.AssemblyDesc == searchString);
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

        public async Task<ContentResult> GetOrigName(string userId)
        {           
            return Content(JsonConvert.SerializeObject(new {Name = (await ishare.GetAccounts()).FirstOrDefault(j => j.UserID == userId).AccName }), "application/json");
        }

        public async Task<IActionResult> Index()
        {
            return View(await DashboardDetGetter());
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
                        return RedirectToAction("Logout", "Login");
                    }
                default:
                    {
                        return RedirectToAction("Logout", "Login");
                    }
            }
        }

        public async Task<IActionResult> MTIList(string whichDoc, string? whichType)
        {
            string type = whichType ?? "MPI";

            TempData["Subj"] = whichDoc;
            TempData["DocType"] = type;

            MTIListModel list = new MTIListModel();
            {
                list.list = (await ishare.GetMTIs()).Where(j => j.Product == whichDoc && j.DocType == type && !j.isDeleted).OrderByDescending(j => j.DateCreated).ToList();
                list.Originators = (await ishare.GetAccounts()).Where(j => j.Role == "ORIGINATOR").Select(j => j.AccName).ToList();
            }

            return View(list);
        }

        public async Task<IActionResult> ShowTravelers()
        {
            Dictionary<string, (string, string)> mtis = (await ishare.GetMTIs()).ToDictionary(j => j.DocumentNumber, j => (j.AssemblyDesc, j.AfterTravLog));
            IEnumerable<ModuleModel> module = await ishare.GetModules();
            IEnumerable<SerialNumberModel> serialNumbers = await ishare.GetSerialNumbers();

            Dictionary<string, (string, string?, string?, string, string, string)> sw = (await ishare.GetStartWork()).Where(j => !j.isObsolete).OrderByDescending(j => j.FinishDate).ToDictionary(j => j.SessionID, j => (j.StartDate.ToShortDateString(), j.FinishDate.HasValue ? j.FinishDate.Value.ToShortDateString() : "NF", j.UserID, j.DocNo, j.SWID.ToString(), j.LogType));
            Dictionary<string, string> accs = (await ishare.GetAccounts()).Where(j => j.Role == "USER").ToDictionary(j => j.UserID, j => j.AccName);

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
                trav.SerialNo = serialNumbers.FirstOrDefault(j => j.SessionId == work.Key).SerialNumber;
                trav.Module = module.FirstOrDefault(j => j.SessionID == work.Key).Module;
                trav.SessionID = work.Key;
                trav.SWID = work.Value.Item5;
                trav.LogType = work.Value.Item6;

                dets.Add(trav);
            }

            res.Travs = dets;
            res.Users = (await ishare.GetAccounts()).Where(j => j.Role == "USER" && !j.isDeleted).Select(j => j.AccName).ToList();

            return View(res);
        }

        public async Task<ContentResult> GetAllDocuments()

        {
            List<MTIModel> docs = (await ishare.GetMTIs()).Where(j => !j.ObsoleteStat).ToList();

            return Content(JsonConvert.SerializeObject(new {r = docs}), "application/json");
        }

        public async Task<ContentResult> GetSessionsCountAsync()
        {
            int rsCount = (await ishare.GetRS()).ToList().Count;
            int usCount = (await ishare.GetStartWork()).Count(j => j.FinishDate == null && !j.isObsolete);

            return Content(JsonConvert.SerializeObject(new {r = rsCount, usCount = usCount}), "application/json");
        }

        private int[] DataPerMonthGetter(List<ProblemLogModel> list)
        {
            int[] res = new int[12];

            foreach (var log in list)
            {
                res[log.LogDate.Month - 1]++;
            }

            return res;
        }

        private async Task<IndexModel> DashboardDetGetter()
        {
            IEnumerable<MTIModel> mtis = await ishare.GetMTIs();
            IEnumerable<ProblemLogModel> pls = (await ishare.GetProblemLogs()).Where(j => j.LogDate.Year == DateTime.Now.Year);

            IndexModel mod = new IndexModel();
            mod.ControlledVal = mtis.Where(j => !j.isDeleted).Count();
            mod.ObsoleteVal = mtis.Count(j => j.ObsoleteStat && !j.isDeleted);

            mod.InterimVal = mtis.Count(j => j.MTPIStatus == 'i');

            mod.PNPCount = mtis.Count(j => j.Product == "PNP" && !j.ObsoleteStat);
            mod.JLPCount = mtis.Count(j => j.Product == "JLP" && !j.ObsoleteStat);
            mod.JTPCount = mtis.Count(j => j.Product == "JTP" && !j.ObsoleteStat);
            mod.OLBCount = mtis.Count(j => j.Product == "OLB" && !j.ObsoleteStat);
            mod.SWAPCount = mtis.Count(j => j.Product == "SWAP" && !j.ObsoleteStat);
            mod.SPARESCount = mtis.Count(j => j.Product == "SPARES" && !j.ObsoleteStat);

            mod.AllDocs = mtis.Where(j => !j.ObsoleteStat).ToList();

            return mod;
        }

        public async Task<ContentResult> GetPLDashboard(int year)
        {
            IEnumerable<ProblemLogModel> pls = (await ishare.GetProblemLogs()).Where(j => j.LogDate.Year == year);

            int JTPVal = pls.Count(j => j.Product == "JTP" && j.Validation == "Valid");
            int JLPVal = pls.Count(j => j.Product == "JLP" && j.Validation == "Valid");
            int OLBVal = pls.Count(j => j.Product == "OLB" && j.Validation == "Valid");
            int PNPVal = pls.Count(j => j.Product == "PNP" && j.Validation == "Valid");
            int SWAPVal = pls.Count(j => j.Product == "SWAP" && j.Validation == "Valid");
            int SPARESVal = pls.Count(j => j.Product == "SPARES" && j.Validation == "Valid");

            int[] openIDPL = DataPerMonthGetter(pls.Where(j => j.PLIDStatus == "OPEN" && j.Validation == "Valid").OrderBy(j => j.LogDate).ToList());
            int[] closedIDPL = DataPerMonthGetter(pls.Where(j => j.PLIDStatus == "CLOSED" && j.Validation == "Valid").OrderBy(j => j.LogDate).ToList());

            int[] openSDPL = DataPerMonthGetter(pls.Where(j => j.PLSDStatus == "OPEN" && j.Validation == "Valid").OrderBy(j => j.LogDate).ToList());
            int[] closedSDPL = DataPerMonthGetter(pls.Where(j => j.PLSDStatus == "CLOSED" && j.Validation == "Valid").OrderBy(j => j.LogDate).ToList());

            return Content(JsonConvert.SerializeObject(new { openIDPL = openIDPL, closedIDPL = closedIDPL, openSDPL = openSDPL, closedSDPL = closedSDPL, jtp = JTPVal, jlp = JLPVal, olb = OLBVal, pnp = PNPVal, swap = SWAPVal, spares = SPARESVal }), "application/json");
        }
    }

    public class IndexModel
    {
        public int ControlledVal { get; set; }
        public int InterimVal { get; set; }
        public int ObsoleteVal { get; set; }
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
