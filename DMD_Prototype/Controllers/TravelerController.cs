using DMD_Prototype.Data;
using DMD_Prototype.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OfficeOpenXml;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace DMD_Prototype.Controllers
{
    public class TravelerController : Controller
    {
        private readonly AppDbContext _Db;
        private readonly ISharedFunct ishared;

        public TravelerController(AppDbContext Db, ISharedFunct ishared)
        {
            _Db = Db;
            this.ishared = ishared;
        }

        public async Task<IActionResult> HoldWork(string sessionId)
        {
            StartWorkModel sw = (await ishared.GetStartWork()).FirstOrDefault(j => j.SessionID == sessionId);

            sw.UserID = "On Hold";

            if (ModelState.IsValid)
            {
                _Db.StartWorkDb.Update(sw);
                _Db.SaveChanges();
            }

            return RedirectToAction("ShowTravelers", "Home");
        }

        public async Task<IActionResult> ChangeTravWorker(int ID, string toWorker)
        {
            StartWorkModel sw = (await ishared.GetStartWork()).FirstOrDefault(j => j.SWID == ID);
            sw.UserID = (await ishared.GetAccounts()).FirstOrDefault(j => j.AccName == toWorker).UserID;

            if(ModelState.IsValid)
            {
                _Db.StartWorkDb.Update(sw);
                _Db.SaveChanges();
            }

            return RedirectToAction("ShowTravelers", "Home");
        }

        public async Task<ContentResult> ValidateWorkTransfer(int ID, string toWorker)
        {
            string response = "go";

            string userId = (await ishared.GetAccounts()).FirstOrDefault(j => j.AccName == toWorker).UserID;

            StartWorkModel sw = (await ishared.GetStartWork()).FirstOrDefault(j => j.UserID == userId && j.FinishDate == null);

            if (sw != null)
            {
                response = "stop";
            }

            string jsonContent = JsonConvert.SerializeObject(new { response = response });
            return Content(jsonContent, "application/json");
        }

        public async Task<ContentResult> GetTravDataForEdit(string sessionId, string docType)
        {
            if (docType.ToLower() == "mpi")
            {
                return Content(await GetTravelerInputs(sessionId, await new WorkController(null, this.ishared).GetMPITemplate(sessionId)), "application/json");
            }
            else
            {
                return Content(await GetTravelerInputs(sessionId, await new WorkController(null, this.ishared).GetMTITemplate(sessionId)), "application/json");
            }
        }

        private async Task<string> GetTravelerInputs(string sessionId, TravelerMPITemplate config)
        {
            List<TravDataForEdit> res = new List<TravDataForEdit>();

            using (ExcelPackage package = new ExcelPackage(Path.Combine(await ishared.GetPath("userDir"), sessionId, await ishared.GetPath("userTravName"))))
            {
                var ws = package.Workbook.Worksheets[0];
                int row = int.Parse(Regex.Replace(config.StartStepNumber, "[^0-9]", ""));
                string stepNumberLocator = Regex.Replace(config.StartStepNumber, "[^a-zA-Z]", "");
                string taskLocator = Regex.Replace(config.StartTask, "[^a-zA-Z]", "");
                string technicianLocator = Regex.Replace(config.StartTechnician, "[^a-zA-Z]", "");
                string dateLocator = Regex.Replace(config.StartDateParameter, "[^a-zA-Z]", "");
                int pageIndexer = 0;

                do
                {
                    if (ws.Cells[$"{dateLocator}{row}"].Value == null)
                    {
                        break;
                    }
                    else if (ws.Cells[$"{dateLocator}{row}"].Value != null)
                    {
                        TravDataForEdit trav = new();
                        trav.Step = ws.Cells[$"{stepNumberLocator}{row}"].Value.ToString();
                        trav.Instruction = ws.Cells[$"{taskLocator}{row}"].Value.ToString();
                        trav.Tech = ws.Cells[$"{technicianLocator}{row}"].Value.ToString();

                        trav.SinglePara = ws.Cells[$"{dateLocator}{row}"].Value.ToString();

                        trav.isMerge = true;

                        res.Add(trav);

                        row += int.Parse(config.IncrementValue);
                    }
                    else if (row > int.Parse(config.LastRow) && package.Workbook.Worksheets.Count > 1)
                    {
                        pageIndexer++;
                        package.Workbook.Worksheets.Add($"P{pageIndexer + 1}");
                        ws = package.Workbook.Worksheets[$"P{pageIndexer + 1}"];
                    }


                } while (true);
            }

            return JsonConvert.SerializeObject(new { r = res });
            
        }

        private async Task<string> GetTravelerInputs(string sessionId, TravelerMTITemplate config)
        {
            List<TravDataForEdit> res = new List<TravDataForEdit>();

            try
            {
                using (ExcelPackage package = new ExcelPackage(Path.Combine(await ishared.GetPath("userDir"), sessionId, await ishared.GetPath("userTravName"))))
                {
                    var ws = package.Workbook.Worksheets[0];
                    int row = int.Parse(Regex.Replace(config.StartStepNumber, "[^0-9]", ""));
                    string stepNumberLocator = Regex.Replace(config.StartStepNumber, "[^a-zA-Z]", "");
                    string taskLocator = Regex.Replace(config.StartTask, "[^a-zA-Z]", "");
                    string technicianAndDateLocator = Regex.Replace(config.StartTechnicianAndDate, "[^a-zA-Z]", "");
                    string parameterLocator = Regex.Replace(config.StartParameter, "[^a-zA-Z]", "");

                    char byThreeChar = char.Parse(Regex.Replace(parameterLocator, "[^a-zA-Z]", ""));
                    int byThreeToASCII = (int)byThreeChar;
                    char byThreeFirst = (char)(byThreeToASCII);
                    char firstByThree = (char)(byThreeToASCII + 1);
                    char secondByThree = (char)(byThreeToASCII + 2);

                    do
                    {
                        if (ws.Cells[$"{parameterLocator}{row}"].Value == null)
                        {
                            break;
                        }

                        TravDataForEdit trav = new();
                        trav.Step = ws.Cells[$"{stepNumberLocator}{row}"].Value.ToString();
                        trav.Instruction = ws.Cells[$"{taskLocator}{row}"].Value.ToString();
                        trav.Tech = ws.Cells[$"{parameterLocator}{row}"].Value.ToString();

                        string startCell = $"{byThreeFirst}{row}";
                        string endCell = $"{secondByThree}{row}";
                        ExcelRangeBase mergeRange = ws.Cells[startCell + ":" + endCell];

                        bool merged = mergeRange.Merge;

                        if (merged)
                        {
                            trav.SinglePara = ws.Cells[$"{parameterLocator}{row}"].Value.ToString();
                            trav.isMerge = true;
                        }
                        else
                        {
                            trav.FirstThreePara = ws.Cells[$"{byThreeFirst}{row}"].Value.ToString();
                            trav.SecondThreePara = ws.Cells[$"{firstByThree}{row}"].Value.ToString();
                            trav.ThirdThreePara = ws.Cells[$"{secondByThree}{row}"].Value.ToString();
                            trav.isMerge = false;
                        }

                        res.Add(trav);

                        row += int.Parse(config.IncrementValue);

                    } while (true);
                }

                
            }
            catch(Exception ex)
            {

            }

            return JsonConvert.SerializeObject(new { r = res });
        }
    }

    public class TravDataForEdit
    {
        public string Step { get; set; } = string.Empty;
        public string Instruction { get; set; } = string.Empty;
        public string? SinglePara { get; set; }
        public string? FirstThreePara { get; set; }
        public string? SecondThreePara { get; set; }
        public string? ThirdThreePara { get; set; }
        public string Tech { get; set; } = string.Empty;
        public bool isMerge { get; set; } = false;
    }
}
