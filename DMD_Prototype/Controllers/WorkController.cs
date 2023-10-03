using DMD_Prototype.Data;
using DMD_Prototype.Models;
using Humanizer.Localisation.TimeToClockNotation;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OfficeOpenXml;
using System.Threading.Tasks;

namespace DMD_Prototype.Controllers
{
    public class WorkController : Controller
    {
        private readonly AppDbContext _Db;
        private readonly List<AccountModel> _accounts;
        private readonly List<PauseWorkModel> _pwmodel;
        private readonly List<StartWorkModel> _swmodel;
        private readonly List<ProblemLogModel> _plModel;

        private string sesID;
        private readonly string userDir = "D:\\jtoledo\\Desktop\\DMD_SessionFolder";
        private readonly string mainDir = "D:\\jtoledo\\Desktop\\DocumentsHere";
        private readonly string travName = "TravelerFileDoNotEdit.xlsx";
        private readonly string userTravName = "Traveler.xlsx";

        public WorkController(AppDbContext _context)
        {
            _Db = _context;
            _accounts = _Db.AccountDb.ToList();
            _pwmodel = _Db.PauseWorkDb.ToList();
            _swmodel = _Db.StartWorkDb.ToList();
            _plModel = _Db.PLDb.ToList();
        }

        //private string SessionIDGetter(string userID)
        //{
        //    return _Db.PauseWorkDb.FirstOrDefault(j => j.Technician == userID && j.RestartDT == null).SessionID;
        //}

        private string UserIDGetter(string name)
        {
            string userid = _accounts.FirstOrDefault(j => j.AccName == name).UserID;

            return userid;
        }

        private void SessionSaver(string docNo, string user)
        {
            StartWorkModel swModel = new StartWorkModel().CreateSW(docNo, UserIDGetter(user));
            sesID = swModel.SessionID;

            if (ModelState.IsValid)
            {
                _Db.StartWorkDb.Add(swModel);
                _Db.SaveChanges();
            }
        }

        private void PauseSession(string sesID, string reason, string tech)
        {
            PauseWorkModel pwModel = new PauseWorkModel().SetPause(sesID, reason, UserIDGetter(tech));

            if (ModelState.IsValid)
            {
                _Db.PauseWorkDb.Add(pwModel);
                _Db.SaveChanges();
            }
        }

        private void CreateNewFolder(string sesID)
        {
            string filePath = Path.Combine(userDir, sesID);
            Directory.CreateDirectory(filePath);
        }

        private void CopyTravToSession(string docNo, string wOrder, string serialNo)
        {
            string filePath = Path.Combine(mainDir, docNo, travName);

            ExcelPackage package = new ExcelPackage(filePath);

            for (int i = 0; i < package.Workbook.Worksheets.Count(); i++)
            {
                var ws = package.Workbook.Worksheets[i];
                ws.Cells[8, 3].Value = DateTime.Now.ToShortDateString();
                ws.Cells[6, 9].Value = wOrder;
                ws.Cells[7, 9].Value = serialNo;
                ws.Cells[1, 10].Value = $"Page {i + 1} of {package.Workbook.Worksheets.Count()}";
            }

            package.SaveAs(Path.Combine(userDir, sesID, userTravName));
        }

        public IActionResult StartWork(string docNo, string EN, string wOrder, string serialNo)
        {
            SessionSaver(docNo, EN);
            CreateNewFolder(sesID);
            CopyTravToSession(docNo, wOrder, serialNo);       

            return RedirectToAction("MTIView", "MTI", new {docuNumber = docNo, workStat = true,
                sesID = sesID, travelerProgress = GetProgressFromTraveler(sesID)});
        }

        public IActionResult PauseWork(string docNo, string EN, string reason, string sessID)
        {
            PauseSession(sessID, reason, EN);

            return RedirectToAction("LoginPage", "Login");
        }

        public IActionResult SubmitTraveler(SubmitTravMod input)
        {
            return RedirectToAction();
        }

        public IActionResult ContinueWork(string userID, bool noPW)
        {
            StartWorkModel swmodel = _swmodel.FirstOrDefault(j => j.UserID == userID && j.FinishDate == null);

            if (noPW)
            {
                CreatePWForNoPW(swmodel.SessionID, userID);
            }

            ContinuePausedWork(swmodel.SessionID);

            return RedirectToAction("MTIView", "MTI", new {docuNumber = swmodel.DocNo, workStat = true, sesID = swmodel.SessionID
            , travelerProgress = GetProgressFromTraveler(swmodel.SessionID)
            });
        }

        public IActionResult FinishWork(string sessionId)
        {
            CompleteWork(sessionId);
            SubmitDateFinished(sessionId);

            return RedirectToAction("Index", "Home");
        }

        private void ContinuePausedWork(string sessionId)
        {
            PauseWorkModel? model = _Db.PauseWorkDb.FirstOrDefault(j => j.SessionID == sessionId && j.RestartDT == null).ContinuePausedSession();

            if (ModelState.IsValid)
            {
                _Db.PauseWorkDb.Update(model);
                _Db.SaveChanges();
            }
        }

        private void SubmitDateFinished(string sessionId)
        {
            string filePath = Path.Combine(userDir, sessionId, userTravName);
            string dateNow = DateTime.Now.ToShortDateString();
            using (ExcelPackage package = new ExcelPackage(filePath))
            {
                for (int i = 0; i < package.Workbook.Worksheets.Count; i++)
                {
                    var ws = package.Workbook.Worksheets[i];
                    ws.Cells[8, 9].Value = dateNow;
                }

                package.Save();
            }
        }

        private void CompleteWork(string sessionId)
        {
            StartWorkModel swModel = _swmodel.FirstOrDefault(j => j.SessionID == sessionId);
            swModel.FinishDate = DateTime.Now;

            if (ModelState.IsValid)
            {
                _Db.StartWorkDb.Update(swModel);
                _Db.SaveChanges();
            }
        }

        private void CreatePWForNoPW(string sessionId, string userId)
        {
            PauseWorkModel pauseWorkModel = new PauseWorkModel().SetPause(sessionId, "Technician did not paused.", userId);

            if (ModelState.IsValid)
            {
                _Db.PauseWorkDb.Add(pauseWorkModel);
                _Db.SaveChanges();
            }
        }

        private string[] GetProgressFromTraveler(string sessionID)
        {
            string[] progress = new string[3];
            string filePath = Path.Combine(userDir, sessionID, "Traveler.xlsx");
            int rowCount = 11;

            using (ExcelPackage package = new ExcelPackage(filePath))
            {
                var worksheet = package.Workbook;
                int pageCount = package.Workbook.Worksheets.Count();
                int sheetCounter = 0;
                do
                {
                    string getTask = worksheet.Worksheets[sheetCounter].Cells[rowCount, 2].Value == null ? "" : worksheet.Worksheets[sheetCounter].Cells[rowCount, 2].Value.ToString();

                    if (string.IsNullOrEmpty(getTask))
                    {
                        if (pageCount <= (sheetCounter + 1))
                        {
                            break;
                        }
                        else
                        {
                            sheetCounter++;
                            rowCount = 10;
                        }
                        
                    }

                    if (!string.IsNullOrEmpty(getTask) &&
                        (worksheet.Worksheets[sheetCounter].Cells[rowCount, 4].Value == null &&
                        worksheet.Worksheets[sheetCounter].Cells[rowCount, 7].Value == null))
                    {
                        progress[0] = worksheet.Worksheets[sheetCounter].Cells[rowCount, 1].Value.ToString();
                        progress[1] = worksheet.Worksheets[sheetCounter].Cells[rowCount, 2].Value.ToString();
                        progress[2] = worksheet.Worksheets[sheetCounter].Cells[rowCount, 12].Value.ToString();

                        break;
                    }

                    rowCount++;

                } while (true);
            }

            return progress;
        }

        private void SaveTravLog(string stepNo, string tAsk, string[]? byThree, string? singlePara, string sessionID, string tech, string date)
        {
            string filePath = Path.Combine(userDir, sessionID, userTravName);
            int rowCount = 11;
            int sheetCounter = 0;

            using(ExcelPackage package = new ExcelPackage(filePath))
            {
                var worksheet = package.Workbook;

                do
                {
                    if (worksheet.Worksheets[sheetCounter].Cells[rowCount, 1].Value == null && worksheet.Worksheets[sheetCounter].Cells[rowCount, 2].Value == null)
                    {
                        sheetCounter++;
                        rowCount = 11;
                    }

                    if (worksheet.Worksheets[sheetCounter].Cells[rowCount, 1].Value.ToString() == stepNo && worksheet.Worksheets[sheetCounter].Cells[rowCount, 2].Value.ToString() == tAsk)
                    {
                        if (byThree.Count() > 0 && byThree != null)
                        {
                            worksheet.Worksheets[sheetCounter].Cells[rowCount, 9].Value = byThree[0];
                            worksheet.Worksheets[sheetCounter].Cells[rowCount, 10].Value = byThree[1];
                            worksheet.Worksheets[sheetCounter].Cells[rowCount, 11].Value = byThree[2];
                        }
                        else
                        {
                            worksheet.Worksheets[sheetCounter].Cells[rowCount, 9].Value = singlePara;
                        }

                        worksheet.Worksheets[sheetCounter].Cells[rowCount, 7].Value = $"{tech}||{date}";

                        break;
                    }

                    rowCount++;
                } while (true);

                package.SaveAs(filePath);
            }
        }

        [HttpPost]
        public ContentResult SubmitTravelerLog(string stepNo, string tAsk, string[]? byThree,
            string? singlePara, string sessionID, string tech, string date)
        {           
            SaveTravLog(stepNo, tAsk, byThree, singlePara, sessionID, tech, date);

            string[] res = GetProgressFromTraveler(sessionID);
            string jsonData = JsonConvert.SerializeObject(new { StepNo = res[0], Task = res[1], Div = res[2] });
            return Content(jsonData, "application/json");
        }

        [HttpPost]
        public ContentResult SubmitProblemLog(string wweek, string affected, string docno,
            string desc, string probcon, string reportedby, string product)
        {

            if (ModelState.IsValid )
            {
                _Db.PLDb.Add(new ProblemLogModel().CreatePL(SetSeries("PL"), DateTime.Now, wweek, affected, product,
                    docno, desc, probcon, reportedby));
                _Db.SaveChanges();
            }

            string jsonData = JsonConvert.SerializeObject("Success");
            return Content(jsonData, "application/json"); // notify originator about the pl
        }

        private string SetSeries(string seriesPrimary)
        {
            string yearNow = DateTime.Now.Year.ToString()[2..];

            List<ProblemLogModel> series = _plModel.Where(j => j.LogDate.Year == DateTime.Now.Year).ToList();

            if (series.Count <= 0)
            {
                return $"PL-{yearNow}-001";
            }
            else
            {
                string lastSeries = series.Last().PLNo.ToString();

                string[] splitSeries = lastSeries.Split('-');

                if (!int.TryParse(splitSeries[2], out int resSeries))
                {
                    throw new ArgumentException("Invalid variable part");
                }

                resSeries++;

                return $"{seriesPrimary}-{yearNow}-{resSeries:000}";
            }

        }
    }

    public class SubmitTravMod
    {
        public string stepNo { get; set; } = string.Empty;
        public string task { get; set; } = string.Empty;
        public string? firstPara { get; set; }
        public string? secPara { get; set; }
        public string? thirdPara { get; set; }
        public string? singlePara { get; set; }
    }
}
