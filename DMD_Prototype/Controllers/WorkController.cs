using DMD_Prototype.Data;
using DMD_Prototype.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OfficeOpenXml;
using DMD_Prototype.Controllers;
using System.Net.Mail;
using System.Net;
using System.Reflection;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using PdfSharp.Charting;
using System.Data;
using System.Net.Sockets;
using Microsoft.Data.SqlClient;

namespace DMD_Prototype.Controllers
{
    public class WorkController : Controller
    {
        private readonly AppDbContext _Db;

        //private string sesID;

        private readonly ISharedFunct ishared;

        public WorkController(AppDbContext _context, ISharedFunct ishared)
        {
            _Db = _context;
            this.ishared = ishared;
        }

        public async Task<ConfigDataTemplate> GetConfigTemplate(string sessionID)
        {
            using (ExcelPackage package = new(Path.Combine(await ishared.GetPath("userDir"), sessionID, await ishared.GetPath("logName"))))
            {
                return JsonConvert.DeserializeObject<ConfigDataTemplate>(package.Workbook.Worksheets["Configuration"].Cells["A1"].Value.ToString());
            }
        }

        public async Task<TELDataTemplate> GetTELTemplate(string sessionID)
        {
            using (ExcelPackage package = new(Path.Combine(await ishared.GetPath("userDir"), sessionID, await ishared.GetPath("logName"))))
            {
                return JsonConvert.DeserializeObject<TELDataTemplate>(package.Workbook.Worksheets["Configuration"].Cells["A1"].Value.ToString());
            }
        }

        public async Task<TravelerMPITemplate> GetMPITemplate(string sessionID)
        {
            return JsonConvert.DeserializeObject<TravelerMPITemplate>(System.IO.File.ReadAllText(await ishared.GetPath("mpiDir")).ToString());
        }

        public async Task<TravelerMTITemplate> GetMTITemplate(string sessionID)
        {
            return JsonConvert.DeserializeObject<TravelerMTITemplate>(System.IO.File.ReadAllText(await ishared.GetPath("mtiDir")).ToString());
        }

        public async Task<string> UserIDGetter(string name)
        {
            string userid = (await ishared.GetAccounts()).FirstOrDefault(j => j.AccName == name).UserID;

            return userid;
        }

        private async Task<string> SessionSaver(string docNo, string user, string wo, string serialNo, string module)
        {
            StartWorkModel swModel = new StartWorkModel().CreateSW(docNo, await UserIDGetter(user), (await ishared.GetMTIs()).FirstOrDefault(j => j.DocumentNumber == docNo).AfterTravLog, $"{wo}");

            if (ModelState.IsValid)
            {
                _Db.ModuleDb.Add(new ModuleModel().CreateModule(swModel.SessionID, module, wo));
                _Db.SerialNumberDb.Add(new SerialNumberModel().SubmitSerialNumber(serialNo, swModel.SessionID));
                _Db.StartWorkDb.Add(swModel);
                _Db.SaveChanges();
            }

            return swModel.SessionID;
        }

        private void PauseSession(string reason, string tech)
        {
            //PauseWorkModel pwModel = new PauseWorkModel().SetPause(sesID, reason, UserIDGetter(tech));

            if (ModelState.IsValid)
            {
                //_Db.PauseWorkDb.Add(pwModel);
                _Db.UADb.Add(new UserActionModel().CreateAction($"Work Paused with reason of {reason}.", tech, DateTime.Now));
                _Db.SaveChanges();
            }
        }

        private async Task CreateNewFolder(string sesID)
        {
            string filePath = Path.Combine(await ishared.GetPath("userDir"), sesID);
            Directory.CreateDirectory(filePath);
        }

        private async Task CopyTravToSession(string docNo, string wOrder, string serialNo, string docType, string sessionID)
        {
            try
            {
                string dateLocator = "";
                string workOrderLocator = "";
                string serialNumberLocator = "";
                string pageLocator = "";

                if (docType == "MPI")
                {
                    TravelerMPITemplate telData = await GetMPITemplate(sessionID);
                    dateLocator = telData.StartDate;
                    workOrderLocator = telData.WorkOrder;
                    serialNumberLocator = telData.SerialNumber;
                    pageLocator = telData.Page;
                }
                else
                {
                    TravelerMTITemplate configData = await GetMTITemplate(sessionID);
                    dateLocator = configData.StartDate;
                    workOrderLocator = configData.WorkOrder;
                    serialNumberLocator = configData.SerialNumber;
                    pageLocator = configData.Page;
                }

                string filePath = Path.Combine(await ishared.GetPath("mainDir"), docNo, await ishared.GetPath("travName"));

                using (ExcelPackage package = new ExcelPackage(filePath))
                {
                    for (int i = 0; i < package.Workbook.Worksheets.Count(); i++)
                    {
                        var ws = package.Workbook.Worksheets[i];
                        ws.Cells[dateLocator].Value = DateTime.Now.ToShortDateString();
                        ws.Cells[workOrderLocator].Value = wOrder;
                        ws.Cells[serialNumberLocator].Value = serialNo;
                        if (!string.IsNullOrEmpty(pageLocator)) ws.Cells[pageLocator].Value = $"Page {i + 1} of {package.Workbook.Worksheets.Count}";
                    }

                    package.SaveAs(Path.Combine(await ishared.GetPath("userDir"), sessionID, await ishared.GetPath("userTravName")));
                }
            }
            catch (Exception ex)
            {

            }
        }

        private async Task<bool> CheckForExistingLogsheet(string ses, string logname)
        {
            return System.IO.File.Exists(Path.Combine(await ishared.GetPath("userDir"), ses, logname));
        }

        public async Task CreateLogsheet(string logType, string sessionId)
        {
            if (await CheckForExistingLogsheet(sessionId, await ishared.GetPath("logName")))
            {
                return;
            }

            string filePath;

            if (logType == "T")
            {
                filePath = await ishared.GetPath("testDir");
            }
            else
            {
                filePath = await ishared.GetPath("configDir");
            }

            ModuleModel module = (await ishared.GetModules()).FirstOrDefault(j => j.SessionID == sessionId);
            SerialNumberModel serialNumber = (await ishared.GetSerialNumbers()).FirstOrDefault(j => j.SessionId == sessionId);

            using (ExcelPackage package = new ExcelPackage(filePath))
            {
                //package.Workbook.Worksheets["Configuration"].Hidden = eWorkSheetHidden.VeryHidden;
                package.Workbook.Worksheets[0].Cells[5, 3].Value = DateTime.Now.ToShortDateString();
                package.Workbook.Worksheets[0].Cells[4, 6].Value = module.WorkOrder;
                package.Workbook.Worksheets[0].Cells[5, 6].Value = serialNumber.SerialNumber;
                package.Workbook.Worksheets[0].Name = "P1";

                package.SaveAs(Path.Combine(await ishared.GetPath("userDir"), sessionId, await ishared.GetPath("logName")));
            }
        }

        public async Task<IActionResult> HoldSession(string sessionID)
        {
            StartWorkModel sw = (await ishared.GetStartWork()).FirstOrDefault(j => j.SessionID == sessionID);

            sw.UserID = string.Empty;

            _Db.StartWorkDb.Update(sw);
            _Db.SaveChanges();

            return RedirectToAction("Index", "Home");
        }

        public async Task<ContentResult> ValidateModule(string module, string serialNo, string workOrder)
        {
            string response = "go";

            SerialNumberModel? serialNumber = (await ishared.GetSerialNumbers()).FirstOrDefault(j => j.SerialNumber == serialNo);
            ModuleModel? getModule = (await ishared.GetModules()).FirstOrDefault(j => j.Module == module);

            if (serialNumber != null)
            {
                return Content(JsonConvert.SerializeObject(new { response = "s", message = $"Serial Number already exist, with Work Order of {getModule.WorkOrder} and Module of {getModule.Module}." }), "application/json");
            }

            if (getModule != null && getModule.WorkOrder != workOrder)
            {
                return Content(JsonConvert.SerializeObject(new { response = "m", message = $"Module already exist, with Work Order of {getModule.WorkOrder}." }), "application/json");
            }

            return Content(JsonConvert.SerializeObject(new { response = response}), "application/json");
        }

        public async Task<IActionResult> StartWorkAsync(string docNo, string EN, string wOrder, string serialNo, string module, string docType)
        {
            string sessionID = await SessionSaver(docNo, EN, wOrder, serialNo, module);
            CreateNewFolder(sessionID);
            CopyTravToSession(docNo, wOrder, serialNo, docType, sessionID);

            return RedirectToAction("MTIView", "MTI", new {docuNumber = docNo, workStat = true, sesID = sessionID});
        }

        public IActionResult PauseWork(string EN, string reason)
        {
            if (ModelState.IsValid)
            {
                _Db.UADb.Add(new UserActionModel().CreateAction($"Work Paused with reason of {reason}.", EN, DateTime.Now));
                _Db.SaveChanges();
            }

            return RedirectToAction("Logout", "Login");
        }

        public async Task<IActionResult> ContinueWork(string userID, bool noPW)
        {
            StartWorkModel swmodel = (await ishared.GetStartWork()).FirstOrDefault(j => j.UserID == userID && j.FinishDate == null);

            if (noPW)
            {
                CreatePWForNoPW(swmodel.SessionID, userID);
            }

            ContinuePausedWork(swmodel.SessionID);

            if ((await ishared.GetMTIs()).FirstOrDefault(j => j.DocumentNumber == swmodel.DocNo).DocType == "MPI")
            {
                return RedirectToAction("MTIView", "MTI", new
                {
                    docuNumber = swmodel.DocNo,
                    workStat = true,
                    sesID = swmodel.SessionID
           ,
                    travelerProgress = GetProgressFromTraveler(swmodel.SessionID, await GetMPITemplate(swmodel.SessionID))
                });
            }
            else
            {
                return RedirectToAction("MTIView", "MTI", new
                {
                    docuNumber = swmodel.DocNo,
                    workStat = true,
                    sesID = swmodel.SessionID
           ,
                    travelerProgress = GetProgressFromTraveler(swmodel.SessionID, await GetMTITemplate(swmodel.SessionID))
                });
            }         
        }

        public async Task<IActionResult> FinishWork(string sessionId, string logType, string docNo)
        {
            await CompleteWork(sessionId);
            
            MTIModel docDet = (await ishared.GetMTIs()).FirstOrDefault(j => j.DocumentNumber == docNo);

            await SubmitDateFinished(sessionId, logType, docDet);

            ModuleModel module = (await ishared.GetModules()).FirstOrDefault(j => j.SessionID == sessionId);
            SerialNumberModel serialNumber = (await ishared.GetSerialNumbers()).FirstOrDefault(j => j.SessionId == sessionId);

            ishared.BackupHandler(logType, whichFileEnum.Traveler, sessionId, $"{docDet.Product} {module.WorkOrder} {module.Module} {serialNumber.SerialNumber}");

            if (logType.ToLower() != "n") ishared.BackupHandler(logType, whichFileEnum.Log, sessionId, $"{docDet.Product} {module.WorkOrder} {module.Module} {serialNumber.SerialNumber}");

            return RedirectToAction("Index", "Home");
        }

        public async Task<ContentResult> SubmitLog(string logcellone, string logcelltwo, string logcellthree, string sessionId, string logType)
        {
            try
            {
                string filePath = Path.Combine(await ishared.GetPath("userDir"), sessionId, await ishared.GetPath("logName"));

                string[] logs = { logcellone, logcelltwo, logcellthree };

                if (logType == "T")
                {
                    await AddLog(logs, await GetTELTemplate(sessionId), filePath);
                }
                else
                {
                    await AddLog(logs, await GetConfigTemplate(sessionId), filePath);
                }                
            }
            catch(Exception ex)
            {
                return Content(JsonConvert.SerializeObject(ex.Message), "application/json");
            }

            return Content(JsonConvert.SerializeObject(null), "application/json");
        }

        private async Task AddLog(string[] log, ConfigDataTemplate config, string filePath)
        {
            try
            {
                int rowCount = int.Parse(Regex.Replace(config.StartPartNumber, "[^0-9]", ""));
                int lastRow = int.Parse(Regex.Replace(config.LastRow, "[^0-9]", ""));
                string partNumberLocator = Regex.Replace(config.StartPartNumber, "[^a-zA-Z]", "");
                string descriptionLocator = Regex.Replace(config.StartDescription, "[^a-zA-Z]", "");
                string serialNumberLocator = Regex.Replace(config.StartSerialNumber, "[^a-zA-Z]", "");
                int incrementValue = int.Parse(Regex.Replace(config.IncrementValue, "[^0-9]", ""));

                using (ExcelPackage package = new ExcelPackage(filePath))
                {
                    int pageCount = 0;

                    string lastPage = package.Workbook.Worksheets.Last().Name;
                    if (lastPage.Contains('P'))
                    {
                        pageCount = int.Parse(Regex.Replace(lastPage, "[^0-9]", "")) - 1;
                    }

                    do
                    {
                        var ws = package.Workbook.Worksheets[$"P{pageCount + 1}"];

                        if (rowCount > lastRow)
                        {
                            rowCount = int.Parse(Regex.Replace(config.StartPartNumber, "[^0-9]", ""));
                            pageCount++;
                            package.Workbook.Worksheets.Add($"P{pageCount + 1}", await GetLogsheetTemplate(""));
                        }
                        else if (ws.Cells[$"{partNumberLocator}{rowCount}"].Value == null)
                        {
                            ws.Cells[$"{partNumberLocator}{rowCount}"].Value = log[0];
                            ws.Cells[$"{descriptionLocator}{rowCount}"].Value = log[1];
                            ws.Cells[$"{serialNumberLocator}{rowCount}"].Value = log[2];

                            break;
                        }
                        else
                        {
                            rowCount += incrementValue;
                        }

                    } while (true);

                    package.Save();
                }
            }
            catch (Exception ex)
            {

            }
        }
        private async Task AddLog(string[] log, TELDataTemplate config, string filePath)
        {
            try
            {
                int rowCount = int.Parse(Regex.Replace(config.StartFAPNumber, "[^0-9]", ""));
                int lastRow = int.Parse(Regex.Replace(config.LastRow, "[^0-9]", ""));
                string partNumberLocator = Regex.Replace(config.StartFAPNumber, "[^a-zA-Z]", "");
                string descriptionLocator = Regex.Replace(config.StartDescription, "[^a-zA-Z]", "");
                string serialNumberLocator = Regex.Replace(config.StartCalibrationDueDate, "[^a-zA-Z]", "");
                int incrementValue = int.Parse(Regex.Replace(config.IncrementValue, "[^0-9]", ""));

                using (ExcelPackage package = new ExcelPackage(filePath))
                {
                    int pageCount = 0;

                    string lastPage = package.Workbook.Worksheets.Last().Name;
                    if (lastPage.Contains('P'))
                    {
                        pageCount = int.Parse(Regex.Replace(lastPage, "[^0-9]", "")) - 1;
                    }

                    do
                    {
                        var ws = package.Workbook.Worksheets[$"P{pageCount + 1}"];

                        if (rowCount > lastRow)
                        {
                            rowCount = int.Parse(Regex.Replace(config.StartFAPNumber, "[^0-9]", ""));
                            pageCount++;
                            package.Workbook.Worksheets.Add($"P{pageCount + 1}", await GetLogsheetTemplate("T"));
                        }
                        else if (ws.Cells[$"{partNumberLocator}{rowCount}"].Value == null)
                        {
                            ws.Cells[$"{partNumberLocator}{rowCount}"].Value = log[0];
                            ws.Cells[$"{descriptionLocator}{rowCount}"].Value = log[1];
                            ws.Cells[$"{serialNumberLocator}{rowCount}"].Value = log[2];

                            break;
                        }
                        else
                        {
                            rowCount += incrementValue;
                        }

                    } while (true);

                    package.Save();
                }
            }
            catch (Exception ex)
            {

            }
        }

        private async Task<ExcelWorksheet> GetLogsheetTemplate(string logType)
        {
            string filePath;

            if (logType == "T")
            {
                filePath = await ishared.GetPath("testDir");
            }
            else
            {
                filePath = await ishared.GetPath("configDir");
            }

            ExcelPackage package = new ExcelPackage(filePath);

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

        private async Task SubmitDateFinished(string sessionId, string logType, MTIModel docDet)
        {
            string filePath = Path.Combine(await ishared.GetPath("userDir"), sessionId, await ishared.GetPath("userTravName"));
            string dateNow = DateTime.Now.ToShortDateString();

            if (docDet.DocType.ToLower() == "mpi")
            {
                TravelerMPITemplate temp = await GetMPITemplate(sessionId);
                FinishTraveler(filePath, dateNow, temp.CompleteDate);
            }
            else
            {
                TravelerMTITemplate temp = await GetMTITemplate(sessionId);
                FinishTraveler(filePath, dateNow, temp.CompleteDate);
            }
       
            if (logType.ToLower() != "n")
            {
                if (logType.ToLower() == "c")
                {
                    FinishLogsheet(sessionId, docDet, await GetConfigTemplate(sessionId));
                }
                else
                {
                    FinishLogsheet(sessionId, docDet, await GetTELTemplate(sessionId));
                }
            }
        }

        private async Task FinishLogsheet(string sessionId, MTIModel docDet, ConfigDataTemplate config)
        {
            using (ExcelPackage package = new ExcelPackage(Path.Combine(await ishared.GetPath("userDir"), sessionId, await ishared.GetPath("logName"))))
            {
                string startDate = package.Workbook.Worksheets[0].Cells[config.StartDate].Value == null ? "Incorrect Traveler Pattern" : package.Workbook.Worksheets.First().Cells[config.StartDate].Value.ToString();
                string workOrder = package.Workbook.Worksheets[0].Cells[config.WorkOrder].Value == null ? "Incorrect Traveler Pattern" : package.Workbook.Worksheets[0].Cells[config.WorkOrder].Value.ToString();
                string serialNumber = package.Workbook.Worksheets[0].Cells[config.SerialNumber].Value == null ? "Incorrect Traveler Pattern" : package.Workbook.Worksheets[0].Cells[config.SerialNumber].Value.ToString();

                for (int i = 0; i < package.Workbook.Worksheets.Count; i++)
                {
                    package.Workbook.Worksheets.Delete("Configuration");
                    var ws = package.Workbook.Worksheets[i];
                    ws.Cells[config.CompleteDate].Value = DateTime.Now.ToShortDateString();                   
                    ws.Cells[config.AssemblyPartNumber].Value = docDet.AssemblyPN;
                    ws.Cells[config.AssemblyDescription].Value = docDet.AssemblyDesc;
                    ws.Cells[config.Reference].Value = docDet.LogsheetDocNo;
                    ws.Cells[config.DocumentControlNumber].Value = docDet.DocumentNumber;
                    ws.Cells[config.RevisionNumber].Value = docDet.LogsheetRevNo;
                    ws.Cells[config.Page].Value = $"{i + 1} of {package.Workbook.Worksheets.Count}";
                    ws.Cells[config.WorkOrder].Value = workOrder;
                    ws.Cells[config.SerialNumber].Value = serialNumber;
                    ws.Cells[config.StartDate].Value = startDate;
                }

                package.Save();
            }
        }

        private async Task FinishLogsheet(string sessionId, MTIModel docDet, TELDataTemplate config)
        {
            using (ExcelPackage package = new ExcelPackage(Path.Combine(await ishared.GetPath("userDir"), sessionId, await ishared.GetPath("logName"))))
            {
                package.Workbook.Worksheets.Delete("Configuration");
                string startDate = package.Workbook.Worksheets[0].Cells[config.StartDate].Value == null ? "Incorrect Traveler Pattern" : package.Workbook.Worksheets.First().Cells[config.StartDate].Value.ToString();
                string workOrder = package.Workbook.Worksheets[0].Cells[config.WorkOrder].Value == null ? "Incorrect Traveler Pattern" : package.Workbook.Worksheets[0].Cells[config.WorkOrder].Value.ToString();
                string serialNumber = package.Workbook.Worksheets[0].Cells[config.SerialNumber].Value == null ? "Incorrect Traveler Pattern" : package.Workbook.Worksheets[0].Cells[config.SerialNumber].Value.ToString();

                for (int i = 0; i < package.Workbook.Worksheets.Count; i++)
                {
                    var ws = package.Workbook.Worksheets[i];
                    ws.Cells[config.CompleteDate].Value = DateTime.Now.ToShortDateString();
                    ws.Cells[config.AssemblyPartNumber].Value = docDet.AssemblyPN;
                    ws.Cells[config.AssemblyDescription].Value = docDet.AssemblyDesc;
                    ws.Cells[config.Reference].Value = docDet.LogsheetDocNo;
                    ws.Cells[config.DocumentControlNumber].Value = docDet.DocumentNumber;
                    ws.Cells[config.RevisionNumber].Value = docDet.LogsheetRevNo;
                    ws.Cells[config.Page].Value = $"{i + 1} of {package.Workbook.Worksheets.Count}";
                    ws.Cells[config.WorkOrder].Value =  workOrder;
                    ws.Cells[config.SerialNumber].Value = serialNumber;
                    ws.Cells[config.StartDate].Value = startDate;
                }
              
                package.Save();
            }
        }

        private async Task FinishTraveler(string filePath, string dateNow, string position)
        {
            using (ExcelPackage package = new ExcelPackage(filePath))
            {
                for (int i = 0; i < package.Workbook.Worksheets.Count; i++)
                {
                    var ws = package.Workbook.Worksheets[i];
                    ws.Cells[position].Value = dateNow;
                }

                package.Save();
            }
        }

        private async Task CompleteWork(string sessionId)
        {
            StartWorkModel swModel = (await ishared.GetStartWork()).FirstOrDefault(j => j.SessionID == sessionId);

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

        public async Task<ContentResult> UserRefreshed(string sessionId, string docType)
        {
            string[] res;

            if (docType == "MPI")
            {
                res = await GetProgressFromTraveler(sessionId, await GetMPITemplate(sessionId));
            }
            else
            {
                res = await GetProgressFromTraveler(sessionId, await GetMTITemplate(sessionId));
            }

            return Content(JsonConvert.SerializeObject(new { StepNo = res[0], Task = res[1], Div = res[2] }), "application/json");
        }

        private async Task<string[]> GetProgressFromTraveler(string? sessionID, TravelerMPITemplate config)
        {
            string[] progress = new string[3];

            if (string.IsNullOrEmpty(sessionID))
            {
                return progress;
            }

            string filePath = Path.Combine(await ishared.GetPath("userDir"), sessionID, await ishared.GetPath("userTravName"));
            int rowCount = int.Parse(Regex.Replace(config.StartStepNumber, "[^0-9]", ""));

            using (ExcelPackage package = new ExcelPackage(filePath))
            {
                var worksheet = package.Workbook;
                int pageCount = package.Workbook.Worksheets.Count();
                int sheetCounter = 0;
                string stepNoLocator = Regex.Replace(config.StartStepNumber, "[^a-zA-Z]", "");
                string taskLocator = Regex.Replace(config.StartTask, "[^a-zA-Z]", "");
                string technicianLocator = Regex.Replace(config.StartTechnician, "[^a-zA-Z]", "");
                string dateLocator = Regex.Replace(config.StartDateParameter, "[^a-zA-Z]", "");

                if (package.Workbook.Worksheets.Count == null || package.Workbook.Worksheets.Count <= 0)
                {
                    return progress;
                }

                do
                {
                    string getTask = worksheet.Worksheets[sheetCounter].Cells[$"{taskLocator}{rowCount}"].Value == null ? "" : worksheet.Worksheets[sheetCounter].Cells[$"{taskLocator}{rowCount}"].Value.ToString();

                    if (string.IsNullOrEmpty(getTask))
                    {
                        if (pageCount <= (sheetCounter + 1))
                        {
                            break;
                        }
                        else
                        {
                            sheetCounter++;
                            rowCount = int.Parse(Regex.Replace(config.StartStepNumber, "[^0-9]", ""));
                        }
                        
                    }

                    if (!string.IsNullOrEmpty(getTask) &&
                        (worksheet.Worksheets[sheetCounter].Cells[$"{technicianLocator}{rowCount}"].Value == null &&
                        worksheet.Worksheets[sheetCounter].Cells[$"{dateLocator}{rowCount}"].Value == null))
                    {
                        progress[0] = worksheet.Worksheets[sheetCounter].Cells[$"{stepNoLocator}{rowCount}"].Value.ToString();
                        progress[1] = worksheet.Worksheets[sheetCounter].Cells[$"{taskLocator}{rowCount}"].Value.ToString();
                        progress[2] = "s";

                        break;
                    }

                    rowCount += int.Parse(config.IncrementValue);

                } while (true);
            }

            return progress;
        }

        private async Task<string[]> GetProgressFromTraveler(string? sessionID, TravelerMTITemplate config)
        {
            string[] progress = new string[3];

            if (string.IsNullOrEmpty(sessionID))
            {
                return progress;
            }

            string filePath = Path.Combine(await ishared.GetPath("userDir"), sessionID, await ishared.GetPath("userTravName"));
            int rowCount = int.Parse(Regex.Replace(config.StartStepNumber, "[^0-9]", ""));

            string stepNoLocator = Regex.Replace(config.StartStepNumber, "[^a-zA-Z]", "");
            string taskLocator = Regex.Replace(config.StartTask, "[^a-zA-Z]", "");
            string technicianAndDateLocator = Regex.Replace(config.StartTechnicianAndDate, "[^a-zA-Z]", "");
            string parameterLocator = Regex.Replace(config.StartParameter, "[^a-zA-Z]", "");

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
                    string getTask = worksheet.Worksheets[sheetCounter].Cells[$"{taskLocator}{rowCount}"].Value == null ? "" : worksheet.Worksheets[sheetCounter].Cells[$"{taskLocator}{rowCount}"].Value.ToString();

                    if (string.IsNullOrEmpty(getTask))
                    {
                        if (pageCount <= (sheetCounter + 1))
                        {
                            break;
                        }
                        else
                        {
                            sheetCounter++;
                            rowCount = int.Parse(Regex.Replace(config.StartStepNumber, "[^0-9]", ""));
                        }

                    }

                    if (!string.IsNullOrEmpty(getTask) &&
                        (worksheet.Worksheets[sheetCounter].Cells[$"{technicianAndDateLocator}{rowCount}"].Value == null &&
                        worksheet.Worksheets[sheetCounter].Cells[$"{parameterLocator}{rowCount}"].Value == null))
                    {
                        progress[0] = worksheet.Worksheets[sheetCounter].Cells[$"{stepNoLocator}{rowCount}"].Value.ToString();
                        progress[1] = worksheet.Worksheets[sheetCounter].Cells[$"{taskLocator}{rowCount}"].Value.ToString();
                        progress[2] = worksheet.Worksheets[sheetCounter].Cells[$"{parameterLocator}{rowCount}"].Merge ? "s" : "t";

                        break;
                    }

                    rowCount += int.Parse(config.IncrementValue);

                } while (true);
            }

            return progress;
        }

        private async Task SaveTravLog(string stepNo, string tAsk, string? singlePara, string sessionID, string tech, TravelerMPITemplate config)
        {
            try
            {
                string filePath = Path.Combine(await ishared.GetPath("userDir"), sessionID, await ishared.GetPath("userTravName"));
                int rowCount = int.Parse(Regex.Replace(config.StartStepNumber, "[^0-9]", ""));
                int sheetCounter = 0;

                using (ExcelPackage package = new ExcelPackage(filePath))
                {
                    var worksheet = package.Workbook;

                    do
                    {
                        if (worksheet.Worksheets[sheetCounter].Cells[$"{Regex.Replace(config.StartStepNumber, "[^a-zA-Z]", "")}{rowCount}"].Value == null)
                        {
                            sheetCounter++;
                            rowCount = int.Parse(Regex.Replace(config.StartStepNumber, "[^a-zA-Z]", ""));
                        }

                        if (worksheet.Worksheets[sheetCounter].Cells[$"{Regex.Replace(config.StartStepNumber, "[^a-zA-Z]", "")}{rowCount}"].Value.ToString() == stepNo && worksheet.Worksheets[sheetCounter].Cells[$"{Regex.Replace(config.StartTask, "[^a-zA-Z]", "")}{rowCount}"].Value.ToString() == tAsk
                            && worksheet.Worksheets[sheetCounter].Cells[$"{Regex.Replace(config.StartTechnician, "[^a-zA-Z]", "")}{rowCount}"].Value == null)
                        {
                            worksheet.Worksheets[sheetCounter].Cells[$"{Regex.Replace(config.StartDateParameter, "[^a-zA-Z]", "")}{rowCount}"].Value = singlePara;
                            worksheet.Worksheets[sheetCounter].Cells[$"{Regex.Replace(config.StartTechnician, "[^a-zA-Z]", "")}{rowCount}"].Value = $"{tech}";
                            worksheet.Worksheets[sheetCounter].Cells[$"{Regex.Replace(config.StartTechnician, "[^a-zA-Z]", "")}{rowCount}"].Style.ShrinkToFit = true;

                            break;
                        }

                        rowCount++;

                    } while (true);

                    package.SaveAs(filePath);
                }
            }
            catch(Exception ex)
            {

            }
        }

        private async Task SaveTravLog(string stepNo, string tAsk, string[]? byThree, string? singlePara, string sessionID, string tech, string date, TravelerMTITemplate config)
        {
            try
            {
                string filePath = Path.Combine(await ishared.GetPath("userDir"), sessionID, await ishared.GetPath("userTravName"));
                int rowCount = 11;
                int sheetCounter = 0;

                char byThreeChar = char.Parse(Regex.Replace(config.StartParameter, "[^a-zA-Z]", ""));
                int byThreeToASCII = (int)byThreeChar;
                char byThreeFirst = (char)(byThreeToASCII);
                char firstByThree = (char)(byThreeToASCII + 1);
                char secondByThree = (char)(byThreeToASCII + 2);

                string stepNumberLocator = Regex.Replace(config.StartStepNumber, "[^a-zA-Z]", "");
                string taskLocator = Regex.Replace(config.StartTask, "[^a-zA-Z]", "");
                string parameterLocator = Regex.Replace(config.StartParameter, "[^a-zA-Z]", "");
                string technicianAndDateLocator = Regex.Replace(config.StartTechnicianAndDate, "[^a-zA-Z]", "");

                using (ExcelPackage package = new ExcelPackage(filePath))
                {
                    var worksheet = package.Workbook;

                    do
                    {
                        if (worksheet.Worksheets[sheetCounter].Cells[$"{stepNumberLocator}{rowCount}"].Value == null)
                        {
                            sheetCounter++;
                            rowCount = 11;
                        }

                        if (worksheet.Worksheets[sheetCounter].Cells[$"{stepNumberLocator}{rowCount}"].Value.ToString() == stepNo && worksheet.Worksheets[sheetCounter].Cells[$"{taskLocator}{rowCount}"].Value.ToString() == tAsk
                            && worksheet.Worksheets[sheetCounter].Cells[$"{parameterLocator}{rowCount}"].Value == null)
                        {
                            if (string.IsNullOrEmpty(singlePara))
                            {
                                worksheet.Worksheets[sheetCounter].Cells[$"{byThreeFirst}{rowCount}"].Value = byThree[0];
                                worksheet.Worksheets[sheetCounter].Cells[$"{firstByThree}{rowCount}"].Value = byThree[1];
                                worksheet.Worksheets[sheetCounter].Cells[$"{secondByThree}{rowCount}"].Value = byThree[2];
                            }
                            else
                            {
                                worksheet.Worksheets[sheetCounter].Cells[$"{parameterLocator}{rowCount}"].Value = singlePara;
                            }

                            worksheet.Worksheets[sheetCounter].Cells[$"{technicianAndDateLocator}{rowCount}"].Value = $"{tech} {date}";

                            worksheet.Worksheets[sheetCounter].Cells[$"{technicianAndDateLocator}{rowCount}"].Style.ShrinkToFit = true;

                            break;
                        }

                        rowCount++;

                    } while (true);

                    package.SaveAs(filePath);
                }
            }
            catch(Exception ex)
            {

            }
        }

        [HttpPost]
        public async Task<ContentResult> SubmitTravelerLog(string stepNo, string tAsk, string[]? byThree,
            string? singlePara, string sessionID, string tech, string date, string docType)
        {
            string[] res;

           if (docType == "MPI")
           {
                SaveTravLog(stepNo, tAsk, singlePara, sessionID, tech, await GetMPITemplate(sessionID));
                res = await GetProgressFromTraveler(sessionID, await GetMPITemplate(sessionID));
            }
           else
           {
                SaveTravLog(stepNo, tAsk, byThree, singlePara, sessionID, tech, date, await GetMTITemplate(sessionID));
                res = await GetProgressFromTraveler(sessionID, await GetMTITemplate(sessionID));
            }
          
            string jsonData = JsonConvert.SerializeObject(new { StepNo = res[0], Task = res[1], Div = res[2] });
            return Content(jsonData, "application/json");
        }

        public async Task<ContentResult> SubmitProblemLog(string wweek, string affected, string docno,
            string desc, string probcon, string reportedby, string product, string rDocNumber)
        {
            string originatorName = (await ishared.GetMTIs()).FirstOrDefault(j => j.DocumentNumber == rDocNumber).OriginatorName;
            AccountModel origModel = (await ishared.GetAccounts()).FirstOrDefault(j => j.UserID == originatorName);
            string origEmail = $"{origModel.Email}{origModel.Sec}{origModel.Dom}";

            ProblemLogModel probModel = new ProblemLogModel();

            if (ModelState.IsValid)
            {
                probModel = new ProblemLogModel().CreatePL(await SetSeries("PL"), DateTime.Now, $"Week {wweek}", affected, product,
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

        private async Task<string> SetSeries(string seriesPrimary)
        {
            string yearNow = DateTime.Now.Year.ToString()[2..];

            List<ProblemLogModel> series = (await ishared.GetProblemLogs()).Where(j => j.LogDate.Year == DateTime.Now.Year).ToList();

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
