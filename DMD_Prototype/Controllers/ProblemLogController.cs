using DMD_Prototype.Data;
using DMD_Prototype.Migrations;
using DMD_Prototype.Models;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OfficeOpenXml;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;
using System.Reflection;

namespace DMD_Prototype.Controllers
{
    public class ProblemLogController : Controller
    {
        private readonly AppDbContext _Db;
        private readonly ISharedFunct ishare;

        private readonly List<ProblemLogModel> _plModel;
        private readonly List<AccountModel> _accounts;

        public ProblemLogController(AppDbContext context, ISharedFunct shared)
        {
            _Db = context;
            _plModel = _Db.PLDb.ToList();
            _accounts = _Db.AccountDb.ToList();
            ishare = shared;
        }

        private string[] GetUsername()
        {
            string[] EN = TempData["EN"] as string[];
            TempData.Keep();

            return EN;
        }

        private Stream GetProblemLogTemplate()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            return assembly.GetManifestResourceStream("DMD_Prototype.wwwroot.Common.Templates.ProblemLogTemplate.xlsx");
        }

        private byte[] ExportPL(List<ProblemLogModel> pls)
        {
            int counter = 10;

            using(ExcelPackage package = new ExcelPackage(GetProblemLogTemplate()))
            {
                var ws = package.Workbook.Worksheets[0];

                foreach (var p in pls)
                {
                    ws.Cells[counter, 1].Value = p.PLNo;
                    ws.Cells[counter, 2].Value = p.LogDate.ToShortDateString();
                    ws.Cells[counter, 3].Value = p.WorkWeek;
                    ws.Cells[counter, 4].Value = p.AffectedDoc;
                    ws.Cells[counter, 5].Value = p.Product;
                    ws.Cells[counter, 6].Value = p.PNDN;
                    ws.Cells[counter, 7].Value = p.Desc;
                    ws.Cells[counter, 8].Value = p.Problem;
                    ws.Cells[counter, 9].Value = p.Reporter;
                    ws.Cells[counter, 10].Value = p.Validation;
                    ws.Cells[counter, 11].Value = p.OwnerRemarks;
                    ws.Cells[counter, 12].Value = p.Category;
                    ws.Cells[counter, 13].Value = p.RC;
                    ws.Cells[counter, 14].Value = p.CA;
                    ws.Cells[counter, 15].Value = p.InterimDoc;
                    ws.Cells[counter, 16].Value = p.IDTCD;
                    ws.Cells[counter, 17].Value = p.IDStatus;
                    ws.Cells[counter, 18].Value = p.StandardizedDoc;
                    ws.Cells[counter, 19].Value = p.SDTCD;
                    ws.Cells[counter, 20].Value = p.SDStatus;
                    ws.Cells[counter, 21].Value = p.Validator;
                    ws.Cells[counter, 22].Value = p.PLIDStatus;
                    ws.Cells[counter, 23].Value = p.PLSDStatus;

                    counter++;
                }

                return package.GetAsByteArray();
            }
        }

        public IActionResult DownloadFile(string selection, DateTime? from, DateTime? to)
        {
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.Headers["Content-Disposition"] = "inline; filename=" + $"ProblemLog_{DateTime.Now.Year}.xlsx";
            return File(ExportPL(GetProblemLogs(selection, from, to)), contentType);
        }

        private List<ProblemLogModel> GetProblemLogs(string selection, DateTime? from, DateTime? to)
        {
            List<ProblemLogModel> pls = new List<ProblemLogModel>();

            switch (selection)
            {
                case "Year":
                    {
                        pls = ishare.GetProblemLogs().Where(j => j.LogDate.Year == DateTime.Now.Year).ToList();
                        break;
                    }
                case "Month":
                    {
                        pls = ishare.GetProblemLogs().Where(j => j.LogDate.Month == DateTime.Now.Month && j.LogDate.Year == DateTime.Now.Year).ToList();
                        break;
                    }
                case "Range":
                    {
                        pls = ishare.GetProblemLogs().Where(j => j.LogDate.Date >= from && j.LogDate.Date <= to).ToList();
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            return pls;
        }

        public ContentResult CheckForPLData(string selection, DateTime? from, DateTime? to)
        {
            List<ProblemLogModel> pls = GetProblemLogs(selection, from, to);

            string convertString = "Selected date will return with zero or no data, action terminated";
            bool plStat = false;

            if (pls.Count > 0 && pls != null)
            {
                convertString = "Download will beggin shortly.";
                plStat = true;

            }

            string jsonContent = JsonConvert.SerializeObject(new { message = convertString, stat =  plStat});
            return Content(jsonContent, "application/json");

        }

        public IActionResult InterimDocValidation(int plID, string PLIDStatus, string PLRemarks, string validator)
        {
            ProblemLogModel pl = _plModel.FirstOrDefault(j => j.PLID == plID);
            string validation = "";

            switch (PLIDStatus)
            {
                case "OPEN":
                    {
                        validation = "DENIED";
                        break;
                    }
                case "CLOSED":
                    {
                        validation = "CLOSED";
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            pl.Validator = validator;
            pl.PLIDStatus = validation;
            pl.PLRemarks = PLRemarks;

            if (ModelState.IsValid)
            {
                ishare.RecordOriginatorAction($"{validator}, validated interim doc with problem log Id of {plID} as {validation}.", validator, DateTime.Now);
                _Db.PLDb.Update(pl);
                _Db.SaveChanges();
            }

            return RedirectToAction("ProblemLogView");
        }

        public IActionResult PermanentDocValidation(int plId, string plStatus, string plRemarks, string validator)
        {
            ProblemLogModel pl = _plModel.FirstOrDefault(j => j.PLID == plId);
            string validation = "";

            switch (plStatus)
            {
                case "OPEN":
                    {
                        validation = "DENIED";
                        break;
                    }
                case "CLOSED":
                    {
                        validation = "CLOSED";
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            pl.PLSDStatus = validation;
            pl.Validator = validator;
            pl.PLRemarks = plRemarks;

            if (ModelState.IsValid)
            {
                ishare.RecordOriginatorAction($"{validator}, validated permanent doc with problem log Id of {plId} as {validation}.", validator, DateTime.Now);
                _Db.PLDb.Update(pl);
                _Db.SaveChanges();
            }

            return RedirectToAction("ProblemLogView");
        }

        public IActionResult EditPLValidation(int plid, string rc, string ca, string? interimdoc, string standardizeddoc, string user)
        {
            ProblemLogModel pl = _plModel.FirstOrDefault(j => j.PLID == plid);

            pl.RC = rc;
            pl.CA = ca;            
            pl.StandardizedDoc = standardizeddoc;

            if (pl.PLIDStatus != "CLOSED")
            {
                pl.InterimDoc = interimdoc;
            }
            if (ModelState.IsValid)
            {
                ishare.RecordOriginatorAction($"{user}, edited/updated problem log with PLID of {pl.PLNo}", user, DateTime.Now);
                _Db.PLDb.Update(pl);
                _Db.SaveChanges();
            }

            return RedirectToAction("ProblemLogView");
        }

        public IActionResult ProblemLogView()
        {
            List<ProblemLogViewModel> pls = new List<ProblemLogViewModel>();

            if (!_plModel.Any())
            {
                return View(pls);
            }

            foreach (var p in _plModel)
            {
                ProblemLogViewModel mod = new ProblemLogViewModel();
                mod.Owner = ishare.GetMTIs().FirstOrDefault(j => j.DocumentNumber == p.DocNo).OriginatorName;
                mod.PL = p;

                pls.Add(mod);
            }

            return View(pls.OrderByDescending(j => j.PL.LogDate));
        }

        public IActionResult SubmitPLValidation(ProblemLogModel fromView)
        {
            string sdVal;

            if (fromView.Validation == "Invalid")
            {
                sdVal = "";
            }
            else
            {
                sdVal = string.IsNullOrEmpty(fromView.StandardizedDoc) ? "No input" : fromView.StandardizedDoc;
            }

            ProblemLogModel pl = _plModel.FirstOrDefault(j => j.PLID == fromView.PLID);
            {
                pl.OwnerRemarks = fromView.OwnerRemarks;
                pl.Category = fromView.Category;
                pl.RC = fromView.RC;
                pl.CA = fromView.CA;
                pl.InterimDoc = fromView.InterimDoc;
                pl.IDTCD = fromView.IDTCD;
                pl.IDStatus = fromView.Validation == "Valid" ? "For Validation" : "";
                pl.StandardizedDoc = sdVal;
                pl.SDTCD = fromView.SDTCD;
                pl.SDStatus = fromView.Validation == "Valid" ? "OPEN" : "";
                pl.Validation = fromView.Validation;

                if (fromView.Validation == "Valid")
                {
                    pl.PLIDStatus = "OPEN";
                    pl.PLSDStatus = "OPEN";
                }
            }

            if (ModelState.IsValid)
            {
                ishare.RecordOriginatorAction($"{fromView.Validator}, validated problem log with PLID of {fromView.PLNo} as {fromView.Validation}.", fromView.Validator, DateTime.Now);
                _Db.PLDb.Update(pl);
                _Db.SaveChanges();
            }

            return RedirectToAction("ProblemLogView");
        }

        public ContentResult GetPLTCDDates(string val)
        {
            DateTime currentDate = DateTime.Now;

            DateTime ID;
            switch (val)
            {
                case "A":
                    {
                        ID = GetWorkingDays(1);
                        break;
                    }
                case "B":
                    {
                        ID = GetWorkingDays(2);
                        break;
                    }
                case "C":
                    {
                        ID = GetWorkingDays(5);
                        break;
                    }
                default:
                    {
                        ID = GetWorkingDays(1);
                        break;
                    }
            }

            DateTime SD = new DateTime(currentDate.Year, currentDate.AddMonths(1).Month, 10);

            string jsonContent = JsonConvert.SerializeObject(new {ID = ID.ToString("yyyy-MM-dd"), SD = SD.ToString("yyyy-MM-dd") });

            return Content(jsonContent, "application/json");
        }

        public ContentResult GetDocStatus(string docNo)
        {
            MTIModel doc = ishare.GetMTIs().FirstOrDefault(j => j.DocumentNumber == docNo);

            return Content(JsonConvert.SerializeObject(new { status = doc.MTPIStatus != 'i' ? "Controlled" : "Interim" }), "application/json");
        }

        public IActionResult GetPLDoc(string docNo)
        {
            using (FileStream fs = new FileStream(Path.Combine(ishare.GetPath("mainDir"), docNo, ishare.GetPath("mainDoc")), FileMode.Open))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    fs.CopyTo(ms);
                    return File(ms.ToArray(), "application/pdf");
                }
            }
        }

        private DateTime GetWorkingDays(int days)
        {
            DateTime res = DateTime.Now;
            int counter = 1;

            do
            {
                res = res.AddDays(1);
                if (res.DayOfWeek != DayOfWeek.Sunday && res.DayOfWeek != DayOfWeek.Saturday)
                {
                    counter++;
                }

            } while (counter <= days);

            return res;
        }
    }

    public class ProblemLogViewModel
    {
        public ProblemLogModel PL { get; set; }
        public string Owner { get; set; }
    }
}
