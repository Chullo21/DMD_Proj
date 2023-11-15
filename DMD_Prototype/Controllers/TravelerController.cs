using DMD_Prototype.Data;
using DMD_Prototype.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OfficeOpenXml;

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

        public IActionResult ChangeTravWorker(int ID, string toWorker)
        {
            StartWorkModel sw = ishared.GetStartWork().FirstOrDefault(j => j.SWID == ID);
            sw.UserID = ishared.GetAccounts().FirstOrDefault(j => j.AccName == toWorker).UserID;

            if(ModelState.IsValid)
            {
                _Db.StartWorkDb.Update(sw);
                _Db.SaveChanges();
            }

            return RedirectToAction("ShowTravelers", "Home");
        }

        public ContentResult ValidateWorkTransfer(int ID, string toWorker)
        {
            string response = "go";

            string userId = ishared.GetAccounts().FirstOrDefault(j => j.AccName == toWorker).UserID;

            StartWorkModel sw = ishared.GetStartWork().FirstOrDefault(j => j.UserID == userId && j.FinishDate == null);

            if (sw != null)
            {
                response = "stop";
            }

            string jsonContent = JsonConvert.SerializeObject(new { response = response });
            return Content(jsonContent, "application/json");
        }

        public ContentResult GetTravDataForEdit(string sessionId)
        {
            List<TravDataForEdit> res = new List<TravDataForEdit>();

            using (ExcelPackage package = new ExcelPackage(Path.Combine(ishared.GetPath("userDir"), sessionId, ishared.GetPath("userTravName"))))
            {
                var ws = package.Workbook.Worksheets[0];
                int row = 11;

                do
                {
                    if (ws.Cells[row, 9].Value == null)
                    {
                        break;
                    }

                    TravDataForEdit trav = new();
                    trav.Step = ws.Cells[row, 1].Value.ToString();
                    trav.Instruction = ws.Cells[row, 2].Value.ToString();
                    trav.Tech = ws.Cells[row, 7].Value.ToString();

                    if (ws.Cells[row, 9, row, 11].Merge)
                    {
                        trav.SinglePara = ws.Cells[row, 9].Value.ToString();
                        trav.isMerge = true;
                    }
                    else
                    {
                        trav.FirstThreePara = ws.Cells[row, 9].Value.ToString();
                        trav.SecondThreePara = ws.Cells[row, 10].Value.ToString();
                        trav.ThirdThreePara = ws.Cells[row, 11].Value.ToString();
                    }

                    res.Add(trav);

                    row++;

                } while (true);
            }

            string jsonContent = JsonConvert.SerializeObject(new { r = res });
            return Content(jsonContent, "application/json");
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
