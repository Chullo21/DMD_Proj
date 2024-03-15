using DMD_Prototype.Data;
using DMD_Prototype.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
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

        private async Task SendEmailNotificationToPLs(string plNo)
        {
            string subject = "Valid Problem Log";
            string body = $"Good day!\r\nAn Originator have validated a problem log with PL number of {plNo} as valid.\r\nPlease refer to our DMD Portal to process this.\r\n\r\nThis is a system generated email, please do not reply. Thank you and have a nice day";
            await ishare.SendEmailNotification((await ishare.GetMultipleusers("PL_INTERVENOR")).ToList(), subject, body);
        }

        private Stream GetProblemLogTemplate()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            return assembly.GetManifestResourceStream("DMD_Prototype.wwwroot.Common.Templates.ProblemLogTemplate.xlsx");
        }

        private byte[] ExportPL(IEnumerable<ProblemLogModel> pls)
        {
            int counter = 10;

            int interimOpenCount = pls.Count(j => j.IDStatus == "OPEN");
            int interimClosedCount = pls.Count(j => j.IDStatus == "CLOSED");
            int standardizedOpenCount = pls.Count(j => j.SDStatus == "OPEN");
            int standardizedClosedCount = pls.Count(j => j.SDStatus == "CLOSED");

            int plInterimOpenCount = pls.Count(j => j.PLIDStatus == "OPEN");
            int plInterimClosedCount = pls.Count(j => j.PLIDStatus == "CLOSED");
            int plStandardizedOpenCount = pls.Count(j => j.PLSDStatus == "OPEN");
            int plStandardizedClosedCount = pls.Count(j => j.PLSDStatus == "CLOSED");

            using (ExcelPackage package = new ExcelPackage(GetProblemLogTemplate()))
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

                    ws.Cells[counter, 1, counter, 23].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    ws.Cells[counter, 1, counter, 23].Style.ShrinkToFit = true;

                    if (p.Validation == "Valid")
                    {
                        ws.Cells[counter, 1, counter, 23].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Red);
                    }
                    else if (p.Validation == "Invalid")
                    {
                        ws.Cells[counter, 1, counter, 23].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Green);
                    }
                    else
                    {
                        ws.Cells[counter, 1, counter, 23].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                    }

                    counter++;
                }

                ws.Cells[5, 2].Value = pls.First().LogDate.Year.ToString();

                ws.Cells[4, 16].Value = interimOpenCount;
                ws.Cells[5, 16].Value = interimClosedCount;
                ws.Cells[4, 19].Value = standardizedOpenCount;
                ws.Cells[5, 19].Value = standardizedClosedCount;

                ws.Cells[4, 22].Value = plInterimOpenCount;
                ws.Cells[4, 23].Value = plInterimClosedCount;
                ws.Cells[5, 22].Value = plStandardizedOpenCount;
                ws.Cells[5, 23].Value = plStandardizedClosedCount;


                return package.GetAsByteArray();
            }
        }

        public async Task<IActionResult> DownloadFile(string selection, DateTime? from, DateTime? to)
        {
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.Headers["Content-Disposition"] = "inline; filename=" + $"ProblemLog_{DateTime.Now.Year}.xlsx";

            return File(ExportPL(await GetProblemLogs(selection, from, to)), contentType);
        }

        private async Task<IEnumerable<ProblemLogModel>> GetProblemLogs(string selection, DateTime? from, DateTime? to)
        {
            List<ProblemLogModel> pls = new List<ProblemLogModel>();

            switch (selection)
            {
                case "Year":
                    {
                        pls = (await ishare.GetProblemLogs()).Where(j => j.LogDate.Year == DateTime.Now.Year).ToList();
                        break;
                    }
                case "Month":
                    {
                        pls = (await ishare.GetProblemLogs()).Where(j => j.LogDate.Month == DateTime.Now.Month && j.LogDate.Year == DateTime.Now.Year).ToList();
                        break;
                    }
                case "Range":
                    {
                        pls = (await ishare.GetProblemLogs()).Where(j => j.LogDate.Date >= from && j.LogDate.Date <= to).ToList();
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            return pls;
        }

        public async Task<ContentResult> CheckForPLData(string selection, DateTime? from, DateTime? to)
        {
            List<ProblemLogModel> pls = (List<ProblemLogModel>)await GetProblemLogs(selection, from, to);

            string convertString = "Selected date will return with zero or no data, action terminated";
            bool plStat = false;

            if (pls.Count > 0 && pls != null)
            {
                convertString = "Download will begin shortly.";
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
            pl.PLIDStatus = PLIDStatus;
            pl.PLRemarks = PLRemarks;
            pl.IDStatus = validation;

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

            pl.PLSDStatus = plStatus;
            pl.Validator = validator;
            pl.PLRemarks = plRemarks;
            pl.PLSDStatus = validation;

            if (ModelState.IsValid)
            {
                ishare.RecordOriginatorAction($"{validator}, validated permanent doc with problem log Id of {plId} as {validation}.", validator, DateTime.Now);
                _Db.PLDb.Update(pl);
                _Db.SaveChanges();
            }

            return RedirectToAction("ProblemLogView");
        }

        public IActionResult EditPLValidation(int plid, string rc, string ca, string? interimdoc, string? standardizeddoc, string user)
        {
            ProblemLogModel pl = _plModel.FirstOrDefault(j => j.PLID == plid);

            pl.RC = rc;
            pl.CA = ca;            
            

            if (pl.PLIDStatus != "CLOSED")
            {
                pl.InterimDoc = interimdoc;
                pl.IDStatus = "For Validation";
            }

            if (pl.SDStatus != "CLOSED")
            {
                pl.StandardizedDoc = standardizeddoc;
                pl.SDStatus = "For Validation";
            }

            if (ModelState.IsValid)
            {
                ishare.RecordOriginatorAction($"{user}, edited/updated problem log with PLID of {pl.PLNo}", user, DateTime.Now);
                _Db.PLDb.Update(pl);
                _Db.SaveChanges();
            }

            return RedirectToAction("ProblemLogView");
        }

        public async Task<IActionResult> ProblemLogView()
        {
            List<ProblemLogViewModel> pls = new List<ProblemLogViewModel>();

            if (!_plModel.Any())
            {
                return View(pls);
            }

            foreach (var p in _plModel)
            {
                ProblemLogViewModel mod = new ProblemLogViewModel();
                mod.Owner = (await ishare.GetMTIs()).FirstOrDefault(j => j.DocumentNumber == p.DocNo).OriginatorName;
                mod.PL = p;

                pls.Add(mod);
            }

            return View(pls.OrderByDescending(j => j.PL.LogDate));
        }

        public IActionResult SubmitPLValidation(ProblemLogModel fromView, char? affectedDocIdentifier)
        {
            string sdVal = "";

            if (fromView.Validation == "Valid")
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
                pl.IDStatus = fromView.Validation == "Valid" ? "OPEN" : "";
                pl.StandardizedDoc = sdVal;
                pl.SDTCD = fromView.SDTCD;
                pl.SDStatus = fromView.Validation == "Valid" ? "OPEN" : "";
                pl.Validation = fromView.Validation;
                pl.AffectedDocSys = affectedDocIdentifier;

                if (fromView.Validation == "Valid")
                {
                    pl.PLIDStatus = "OPEN";
                    pl.PLSDStatus = "OPEN";
                }
            }

            if (affectedDocIdentifier == 'd' || affectedDocIdentifier == 'b' || affectedDocIdentifier == 's')
            {
                pl.IDStatus = "CLOSED";
                pl.PLIDStatus = "CLOSED";
            }

            if (ModelState.IsValid)
            {
                ishare.RecordOriginatorAction($"{fromView.Validator}, validated problem log with PLID of {pl.PLNo} as {pl.Validation}.", fromView.Validator, DateTime.Now);
                _Db.PLDb.Update(pl);
                _Db.SaveChanges();

                if (fromView.Validation == "Valid")
                {
                    SendEmailNotificationToPLs(pl.PLNo);
                }
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

        public async Task<ContentResult> GetDocStatus(string docNo)
        {
            MTIModel doc = (await ishare.GetMTIs()).FirstOrDefault(j => j.DocumentNumber == docNo);

            return Content(JsonConvert.SerializeObject(new { status = doc.MTPIStatus != 'i' ? "Controlled" : "Interim" }), "application/json");
        }

        public async Task<IActionResult> GetPLDoc(string docNo, int plID)
        {
            string whichDoc = string.Empty;

            ProblemLogModel pl = (await ishare.GetProblemLogs()).FirstOrDefault(j => j.PLID == plID);

            switch (pl.AffectedDocSys)
            {
                case 'm':
                    {
                        whichDoc = "mainDoc";
                        break;
                    }
                case 'b':
                    {
                        whichDoc = "bom";
                        break;
                    }
                case 's':
                    {
                        whichDoc = "schema";
                        break;
                    }
                case 'd':
                    {
                        whichDoc = "assy";
                        break;
                    }                   
            }
            using (FileStream fs = new FileStream(Path.Combine(await ishare.GetPath("mainDir"), docNo, await ishare.GetPath(whichDoc)), FileMode.Open))
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

    public class PLEmails
    {
        public string Email { get; set; }
        public string Sec { get; set; }
        public string Dom { get; set; }
    }
}
