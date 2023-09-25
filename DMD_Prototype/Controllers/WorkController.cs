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

        private string sesID;
        private readonly string mainDir = "D:\\jtoledo\\Desktop\\DMD_SessionFolder";
        private readonly string travName = "Traveler.xlsx";

        public WorkController(AppDbContext _context)
        {
            _Db = _context;
            _accounts = _Db.AccountDb.ToList();
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
                //_Db.SaveChanges();
            }
        }

        private void PauseSession(string sesID, string reason, string tech)
        {
            PauseWorkModel pwModel = new PauseWorkModel().SetPause(sesID, reason, tech);

            if (ModelState.IsValid)
            {
                _Db.PauseWorkDb.Add(pwModel);
                //_Db.SaveChanges();
            }
        }

        private void CreateNewFolder(string sesID)
        {
            string filePath = Path.Combine(mainDir, sesID);
            Directory.CreateDirectory(filePath);

            using(ExcelPackage package = new ExcelPackage())
            {
                package.Workbook.Worksheets.Add("Traveler");

                package.SaveAs(Path.Combine(filePath, travName));
            }
        }

        public IActionResult StartWork(string docNo, string EN)
        {
            SessionSaver(docNo, EN);

            CreateNewFolder(sesID);

            return RedirectToAction("MTIView", "MTI", new {docuNumber = docNo, workStat = true, sesID = sesID});
        }

        public IActionResult PauseWork(string docNo, string EN, string reason, string sesID)
        {
            PauseSession(sesID, reason, EN);

            return RedirectToAction();
        }

        public IActionResult SubmitTraveler(SubmitTravMod input)
        {
            return RedirectToAction();
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
