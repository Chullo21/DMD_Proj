﻿using DMD_Prototype.Data;
using DMD_Prototype.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OfficeOpenXml;
using DMD_Prototype.Controllers;
using System.Net.Mail;
using System.Net;
using System.Reflection;

namespace DMD_Prototype.Controllers
{
    public class WorkController : Controller
    {
        private readonly AppDbContext _Db;

        private string sesID;

        private readonly ISharedFunct ishared;

        public WorkController(AppDbContext _context, ISharedFunct ishared)
        {
            _Db = _context;
            this.ishared = ishared;
        }

        private string UserIDGetter(string name)
        {
            string userid = ishared.GetAccounts().FirstOrDefault(j => j.AccName == name).UserID;

            return userid;
        }

        private void SessionSaver(string docNo, string user, string wo, string serialNo, string module)
        {
            StartWorkModel swModel = new StartWorkModel().CreateSW(docNo, UserIDGetter(user), ishared.GetMTIs().FirstOrDefault(j => j.DocumentNumber == docNo).AfterTravLog);
            sesID = swModel.SessionID;

            if (ModelState.IsValid)
            {
                _Db.ModuleDb.Add(new ModuleModel().CreateModule(sesID, module, serialNo, wo));
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
            string filePath = Path.Combine(ishared.GetPath("userDir"), sesID);
            Directory.CreateDirectory(filePath);
        }

        private void CopyTravToSession(string docNo, string wOrder, string serialNo)
        {
            string filePath = Path.Combine(ishared.GetPath("mainDir"), docNo, ishared.GetPath("travName"));

            using(ExcelPackage package = new ExcelPackage(filePath))
            {
                for (int i = 0; i < package.Workbook.Worksheets.Count(); i++)
                {
                    var ws = package.Workbook.Worksheets[i];
                    ws.Cells[8, 3].Value = DateTime.Now.ToShortDateString();
                    ws.Cells[6, 9].Value = wOrder;
                    ws.Cells[7, 9].Value = serialNo;
                    ws.Cells[1, 10].Value = $"Page {i + 1} of {package.Workbook.Worksheets.Count}";
                }

                package.SaveAs(Path.Combine(ishared.GetPath("userDir"), sesID, ishared.GetPath("userTravName")));
            }
        }

        private bool CheckForExistingLogsheet(string ses, string logname)
        {
            return System.IO.File.Exists(Path.Combine(ishared.GetPath("userDir"), ses, logname));
        }

        public void CreateLogsheet(string logType, string sessionId)
        {
            if (CheckForExistingLogsheet(sessionId, ishared.GetPath("logName")))
            {
                return;
            }

            string filePath;

            if (logType == "T")
            {
                filePath = "DMD_Prototype.wwwroot.Common.Templates.TEL.xlsx";
            }
            else
            {
                filePath = "DMD_Prototype.wwwroot.Common.Templates.CL.xlsx";
            }

            Assembly assembly = Assembly.GetExecutingAssembly();

            Stream stream = assembly.GetManifestResourceStream(filePath);

            ModuleModel module = ishared.GetModules().FirstOrDefault(j => j.SessionID == sessionId);

            using (ExcelPackage package = new ExcelPackage(stream))
            {
                package.Workbook.Worksheets[0].Cells[5, 3].Value = DateTime.Now.ToShortDateString();
                package.Workbook.Worksheets[0].Cells[4, 6].Value = module.WorkOrder;
                package.Workbook.Worksheets[0].Cells[5, 6].Value = module.SerialNo;

                package.SaveAs(Path.Combine(ishared.GetPath("userDir"), sessionId, ishared.GetPath("logName")));
            }
        }

        public IActionResult HoldSession(string sessionID)
        {
            StartWorkModel sw = ishared.GetStartWork().FirstOrDefault(j => j.SessionID == sessionID);

            sw.UserID = string.Empty;

            _Db.StartWorkDb.Update(sw);
            _Db.SaveChanges();

            return RedirectToAction("Index", "Home");
        }

        public ContentResult ValidateModule(string module, string serialNo, string workOrder)
        {
            string response = "go";

            ModuleModel mod = ishared.GetModules().FirstOrDefault(j => j.Module == module && j.WorkOrder != workOrder);

            if (mod != null)
            {
                return Content(JsonConvert.SerializeObject(new { response = "m" }), "application/json");
            }

            IEnumerable<ModuleModel> wO = ishared.GetModules().Where(j => j.WorkOrder == workOrder);

            Dictionary<string, string> modules = wO.Where(j => j.Module == module).ToDictionary(j => j.Module, j => j.SerialNo);

            if (modules.Any(j => j.Value == serialNo))
            {
                response = "s";
            }

            return Content(JsonConvert.SerializeObject(new { response = response }), "application/json");
        }

        public IActionResult StartWork(string docNo, string EN, string wOrder, string serialNo, string module)
        {
            SessionSaver(docNo, EN, wOrder, serialNo, module);
            CreateNewFolder(sesID);
            CopyTravToSession(docNo, wOrder, serialNo);

            return RedirectToAction("MTIView", "MTI", new {docuNumber = docNo, workStat = true,
                sesID = sesID});
        }

        public IActionResult PauseWork(string docNo, string EN, string reason, string sessID)
        {
            PauseSession(sessID, reason, EN);

            return RedirectToAction("LoginPage", "Login");
        }

        //public IActionResult SubmitTraveler(SubmitTravMod input)
        //{
        //    return RedirectToAction();
        //}

        public IActionResult ContinueWork(string userID, bool noPW)
        {
            StartWorkModel swmodel = ishared.GetStartWork().FirstOrDefault(j => j.UserID == userID && j.FinishDate == null);

            if (noPW)
            {
                CreatePWForNoPW(swmodel.SessionID, userID);
            }

            ContinuePausedWork(swmodel.SessionID);

            return RedirectToAction("MTIView", "MTI", new {docuNumber = swmodel.DocNo, workStat = true, sesID = swmodel.SessionID
            , travelerProgress = GetProgressFromTraveler(swmodel.SessionID)
            });
        }

        public IActionResult FinishWork(string sessionId, string logType, string docNo)
        {
            CompleteWork(sessionId);
            SubmitDateFinished(sessionId, logType, docNo);

            MTIModel docDet = ishared.GetMTIs().FirstOrDefault(j => j.DocumentNumber == docNo);
            ModuleModel module = ishared.GetModules().FirstOrDefault(j => j.SessionID == sessionId);

            ishared.BackupHandler(logType, whichFileEnum.Traveler, sessionId, $"{docDet.Product} {module.WorkOrder} {module.Module} {module.SerialNo}");

            if (logType.ToLower() != "n") ishared.BackupHandler(logType, whichFileEnum.Log, sessionId, $"{docDet.Product} {module.WorkOrder} {module.Module} {module.SerialNo}");

            return RedirectToAction("Index", "Home");
        }

        public ContentResult SubmitLog(string logcellone, string logcelltwo, string logcellthree, string sessionId, string logType)
        {
            string filePath = Path.Combine(ishared.GetPath("userDir"), sessionId, ishared.GetPath("logName"));

            int rowCount = 10;

            using(ExcelPackage package = new ExcelPackage(filePath))
            {
                int pageCount = package.Workbook.Worksheets.Count <= 1 ? 0 : package.Workbook.Worksheets.Count - 1;

                do
                {
                    var ws = package.Workbook.Worksheets[pageCount];

                    if (rowCount >= 46 && ws.Cells[rowCount, 1].Value != null)
                    {
                        rowCount = 7;
                        pageCount++;
                        package.Workbook.Worksheets.Add($"P{pageCount + 1}", GetLogsheetTemplate(logType));
                    }
                   
                    if (ws.Cells[rowCount, 1].Value == null)
                    {
                        ws.Cells[rowCount, 1].Value = logcellone;
                        ws.Cells[rowCount, 3].Value = logcelltwo;
                        ws.Cells[rowCount, 7].Value = logcellthree;

                        break;
                    }

                    rowCount += 3;

                } while (true);

                package.Save();
            }

            return Content(JsonConvert.SerializeObject(null), "application/json");
        }

        private ExcelWorksheet GetLogsheetTemplate(string logType)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string filePath;

            if (logType == "T")
            {
                filePath = "DMD_Prototype.wwwroot.Common.Templates.TEL.xlsx";
            }
            else
            {
                filePath = "DMD_Prototype.wwwroot.Common.Templates.CL.xlsx";
            }

            Stream stream = assembly.GetManifestResourceStream(filePath);

            ExcelPackage package = new ExcelPackage(stream);

            return package.Workbook.Worksheets[0];
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

        private void SubmitDateFinished(string sessionId, string logType, string docNo)
        {
            MTIModel mTIModel = ishared.GetMTIs().FirstOrDefault(j => j.DocumentNumber == docNo);

            string filePath = Path.Combine(ishared.GetPath("userDir"), sessionId, ishared.GetPath("userTravName"));
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

            if (logType != "N")
            {
                using (ExcelPackage package = new ExcelPackage(Path.Combine(ishared.GetPath("userDir"), sessionId, ishared.GetPath("logName"))))
                {
                    string startDate = package.Workbook.Worksheets.First().Cells[5, 3].Value.ToString();

                    for (int i = 0; i < package.Workbook.Worksheets.Count; i++)
                    {
                        var ws = package.Workbook.Worksheets[i];
                        ws.Cells[6, 3].Value = dateNow;
                        ws.Cells[5, 3].Value = startDate;
                        ws.Cells[3, 3].Value = mTIModel.AssemblyPN;
                        ws.Cells[4, 3].Value = mTIModel.AssemblyDesc;
                        ws.Cells[2, 5].Value = mTIModel.LogsheetDocNo;
                        ws.Cells[2, 8].Value = mTIModel.DocumentNumber;
                        ws.Cells[3, 6].Value = mTIModel.LogsheetRevNo;
                        ws.Cells[3, 9].Value = $"{i + 1} of {package.Workbook.Worksheets.Count}";
                    }

                    package.Save();
                }
            }
        }

        private void CompleteWork(string sessionId)
        {
            StartWorkModel swModel = ishared.GetStartWork().FirstOrDefault(j => j.SessionID == sessionId);

            swModel.FinishDate = DateTime.Now;

            _Db.StartWorkDb.Update(swModel);
            _Db.SaveChanges();
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

        public ContentResult UserRefreshed(string sessionId)
        {
            string[] res = GetProgressFromTraveler(sessionId);
            return Content(JsonConvert.SerializeObject(new { StepNo = res[0], Task = res[1], Div = res[2] }), "application/json");
        }

        private string[] GetProgressFromTraveler(string? sessionID)
        {
            string[] progress = new string[3];

            if (string.IsNullOrEmpty(sessionID))
            {
                return progress;
            }

            string filePath = Path.Combine(ishared.GetPath("userDir"), sessionID, ishared.GetPath("userTravName"));
            int rowCount = 11;

            using (ExcelPackage package = new ExcelPackage(filePath))
            {
                var worksheet = package.Workbook;
                int pageCount = package.Workbook.Worksheets.Count();
                int sheetCounter = 0;

                if (package.Workbook.Worksheets.Count == null || package.Workbook.Worksheets.Count <= 0)
                {
                    return progress;
                }

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
                        progress[2] = worksheet.Worksheets[sheetCounter].Cells[rowCount, 9].Merge ? "s" : "t";

                        break;
                    }

                    rowCount++;

                } while (true);
            }

            return progress;
        }

        private void SaveTravLog(string stepNo, string tAsk, string[]? byThree, string? singlePara, string sessionID, string tech, string date, string docType)
        {
            string filePath = Path.Combine(ishared.GetPath("userDir"), sessionID, ishared.GetPath("userTravName"));
            int rowCount = 11;
            int sheetCounter = 0;

            using(ExcelPackage package = new ExcelPackage(filePath))
            {
                var worksheet = package.Workbook;

                do
                {
                    if (worksheet.Worksheets[sheetCounter].Cells[rowCount, 1].Value == null)
                    {
                        sheetCounter++;
                        rowCount = 11;
                    }

                    if (worksheet.Worksheets[sheetCounter].Cells[rowCount, 1].Value.ToString() == stepNo && worksheet.Worksheets[sheetCounter].Cells[rowCount, 2].Value.ToString() == tAsk
                        && worksheet.Worksheets[sheetCounter].Cells[rowCount, 9].Value == null)
                    {
                        if (string.IsNullOrEmpty(singlePara))
                        {
                            worksheet.Worksheets[sheetCounter].Cells[rowCount, 9].Value = byThree[0];
                            worksheet.Worksheets[sheetCounter].Cells[rowCount, 10].Value = byThree[1];
                            worksheet.Worksheets[sheetCounter].Cells[rowCount, 11].Value = byThree[2];
                        }
                        else
                        {
                            worksheet.Worksheets[sheetCounter].Cells[rowCount, 9].Value = singlePara;
                        }

                        if (docType == "MTI")
                        {
                            worksheet.Worksheets[sheetCounter].Cells[rowCount, 7].Value = $"{tech}||{date}";
                        }
                        else
                        {
                            worksheet.Worksheets[sheetCounter].Cells[rowCount, 7].Value = $"{tech}";
                        }

                        worksheet.Worksheets[sheetCounter].Cells[rowCount, 7].Style.ShrinkToFit = true;

                        break;
                    }

                    rowCount++;

                } while (true);

                package.SaveAs(filePath);
            }
        }

        [HttpPost]
        public ContentResult SubmitTravelerLog(string stepNo, string tAsk, string[]? byThree,
            string? singlePara, string sessionID, string tech, string date, string docType)
        {           
            SaveTravLog(stepNo, tAsk, byThree, singlePara, sessionID, tech, date, docType);

            string[] res = GetProgressFromTraveler(sessionID);
            string jsonData = JsonConvert.SerializeObject(new { StepNo = res[0], Task = res[1], Div = res[2] });
            return Content(jsonData, "application/json");
        }

        public ContentResult SubmitProblemLog(string wweek, string affected, string docno,
            string desc, string probcon, string reportedby, string product, string rDocNumber)
        {
            AccountModel origModel = ishared.GetAccounts().FirstOrDefault(j => j.UserID == ishared.GetMTIs().FirstOrDefault(j => j.DocumentNumber == rDocNumber).OriginatorName);
            string origEmail = $"{origModel.Email}{origModel.Sec}{origModel.Dom}";

            ProblemLogModel probModel = new ProblemLogModel();

            if (ModelState.IsValid)
            {
                probModel = new ProblemLogModel().CreatePL(SetSeries("PL"), DateTime.Now, $"Week {wweek}", affected, product,
                    docno, desc, probcon, reportedby, rDocNumber);
                _Db.PLDb.Add(probModel);
                _Db.SaveChanges();
            }

            if (!string.IsNullOrEmpty(origEmail))
            {
                string subject = "New Problem Log";
                string body = $"Good day!\r\nYou have received a problem log from a technician, having a PL number of {probModel.PLNo}. Refer below:\r\n\r\nWork week: {wweek}\r\nAffected Document: {affected}\r\nDocument Number: {rDocNumber}\r\nDescription: {desc}\r\nProblem Description: {probcon}\r\nReporter: {reportedby}\r\nProduct: {product}" +
                    "\r\n\r\nThis is a system generated email, please do not reply. Thank you and have a great day!";

                SendEmail(origEmail, subject, body);
            }

            string jsonData = JsonConvert.SerializeObject(new {response = "Success"});
            return Content(jsonData, "application/json");
        }

        private void SendEmail(string origEmail, string subject, string body)
        {
            ishared.SendEmailNotification(origEmail, subject, body);
        }

        private string SetSeries(string seriesPrimary)
        {
            string yearNow = DateTime.Now.Year.ToString()[2..];

            List<ProblemLogModel> series = ishared.GetProblemLogs().Where(j => j.LogDate.Year == DateTime.Now.Year).ToList();

            if (series.Count <= 0)
            {
                return $"PL-{yearNow}-001";
            }
            else
            {
                string lastSeries = series.Last().PLNo.ToString();

                string[] splitSeries = lastSeries.Split('-');

                int resSeries = int.Parse(splitSeries[2]);

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
