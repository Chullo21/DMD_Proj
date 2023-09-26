using DMD_Prototype.Data;
using DMD_Prototype.Models;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

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

        private string UserIDGetter(string name)
        {
            string userid = _accounts.FirstOrDefault(j => j.AccName == name).UserID;

            return userid;
        }

        private void SessionSaver(string docNo, string user)
        {
            StartWorkModel swModel = new StartWorkModel(docNo, UserIDGetter(user));
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

            return RedirectToAction("MTIView", "MTI", new {docuNumber = docNo, workStat = true, sesID = sesID});
        }

        public IActionResult PauseWork(string docNo, string EN, string reason, string sesID)
        {
            PauseSession(sesID, reason, EN);

            return RedirectToAction("LoginPage", "Login");
        }

        public IActionResult SubmitTraveler(SubmitTravMod input)
        {
            return RedirectToAction();
        }

        public IActionResult ContinueWork(string userID)
        {
            StartWorkModel swmodel = _swmodel.FirstOrDefault(j => j.UserID == userID && j.FinishDate == null);

            return RedirectToAction("MTIView", "MTI", new {docuNumber = swmodel.DocNo, workStat = true, sesID = swmodel.SessionID});
        }

        [HttpPost]
        public JsonResult SubmitTravelerLog()
        {


            return Json("Something went wrong");
        }

        [HttpPost]
        public JsonResult SubmitProblemLog()
        {
            

            return Json("Something went wrong");
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
