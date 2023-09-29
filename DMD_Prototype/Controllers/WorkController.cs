using DMD_Prototype.Data;
using DMD_Prototype.Models;
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
        }

        private string SessionIDGetter(string userID)
        {
            return _Db.PauseWorkDb.FirstOrDefault(j => j.Technician == userID && j.RestartDT == null).SessionID;
        }

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

            using(ExcelPackage package = new ExcelPackage())
            {
                package.Workbook.Worksheets.Add("P1");

                package.SaveAs(Path.Combine(filePath, userTravName));
            }
        }

        private List<string[]> CopyTravelerToParticular(string docNo)
        {
            List<string[]> res = new List<string[]>();
            int rowCount = 1;
            string filePath = Path.Combine(mainDir, docNo, travName);

            using (ExcelPackage package = new ExcelPackage(filePath))
            {
                var worksheet = package.Workbook.Worksheets.First();

                do
                {
                    if (worksheet.Cells[rowCount, 1].Value != null)
                    {
                        string[] addThis = new string[3];
                        addThis[0] = worksheet.Cells[rowCount, 1].Value.ToString();
                        addThis[1] = worksheet.Cells[rowCount, 2].Value.ToString();
                        addThis[2] = worksheet.Cells[rowCount, 3].Value.ToString();

                        res.Add(addThis);
                        rowCount++;
                    }
                    else
                    {
                        break;
                    }

                } while (true);

                return res;
            }
        }

        private void CopyTravelerToSessionFolder(string docNo)
        {
            string[] progress = new string[2];
            string filePath = Path.Combine(userDir, sesID, userTravName);
            int rowCount = 1;

            using (ExcelPackage package = new ExcelPackage(filePath))
            {
                package.Workbook.Worksheets.Add("Traveler");
                package.Workbook.Worksheets.Delete(package.Workbook.Worksheets.FirstOrDefault(j => j.Name == "P1"));
                

                var worksheet = package.Workbook.Worksheets[0];

                foreach (string[] item in CopyTravelerToParticular(docNo))
                {
                    worksheet.Cells[rowCount, 1].Value = item[0];
                    worksheet.Cells[rowCount, 2].Value = item[1];
                    worksheet.Cells[rowCount, 3].Value = item[2];

                    rowCount++;
                }

                package.SaveAs(filePath);
            }
        }

        public IActionResult StartWork(string docNo, string EN)
        {
            SessionSaver(docNo, EN);
            CreateNewFolder(sesID);
            CopyTravelerToSessionFolder(docNo);           

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
            int rowCount = 1;

            using (ExcelPackage package = new ExcelPackage(filePath))
            {
                var worksheet = package.Workbook.Worksheets[0];

                do
                {
                    if (worksheet.Cells[rowCount, 2].Value == null)
                    {
                        break;
                    }

                    if (worksheet.Cells[rowCount, 2].Value != null && (worksheet.Cells[rowCount, 4].Value == null && worksheet.Cells[rowCount, 7].Value == null))
                    {
                        progress[0] = worksheet.Cells[rowCount, 1].Value.ToString();
                        progress[1] = worksheet.Cells[rowCount, 2].Value.ToString();
                        progress[2] = worksheet.Cells[rowCount, 3].Value.ToString();

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
            int rowCount = 1;

            using(ExcelPackage package = new ExcelPackage(filePath))
            {
                var worksheet = package.Workbook.Worksheets[0];

                do
                {
                    if (worksheet.Cells[rowCount, 1].Value.ToString() == stepNo && worksheet.Cells[rowCount, 2].Value.ToString() == tAsk)
                    {
                        if (byThree.Count() > 0 && byThree != null)
                        {
                            worksheet.Cells[rowCount, 4].Value = byThree[0];
                            worksheet.Cells[rowCount, 5].Value = byThree[1];
                            worksheet.Cells[rowCount, 6].Value = byThree[2];
                        }
                        else
                        {
                            worksheet.Cells[rowCount, 7].Value = singlePara;
                        }

                        worksheet.Cells[rowCount, 8].Value = tech;
                        worksheet.Cells[rowCount, 9].Value = date;

                        break;
                    }

                    rowCount++;
                } while (true);

                package.SaveAs(filePath);
            }
        }

        [HttpPost]
        public ContentResult SubmitTravelerLog(string stepNo, string tAsk, string[]? byThree, string? singlePara, string sessionID, string tech, string date)
        {           
            SaveTravLog(stepNo, tAsk, byThree, singlePara, sessionID, tech, date);

            string[] res = GetProgressFromTraveler(sessionID);
            string jsonData = JsonConvert.SerializeObject(new { StepNo = res[0], Task = res[1], Div = res[2] });
            return Content(jsonData, "application/json");

        }

        [HttpPost]
        public JsonResult SubmitProblemLog(string logdate, string affected, string docno, string partno, string desc, string probcon, string reportedby)
        {
            ProblemLogModel pl = new ProblemLogModel().CreatePL();

            return Json("");
        }

        private string SetPLSeries()
        {
            return "";
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
