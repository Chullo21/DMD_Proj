﻿using DMD_Prototype.Data;
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

        public ContentResult GetAllDocuments()

        {
            List<MTIModel> docs = ishare.GetMTIs().Where(j => !j.ObsoleteStat).ToList();

            return Content(JsonConvert.SerializeObject(new {r = docs}), "application/json");
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

        public List<MTIModel>? AllDocs { get; set; }
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
