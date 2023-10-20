using DMD_Prototype.Data;
using DMD_Prototype.Models;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

namespace DMD_Prototype.Controllers
{   
    public interface ISharedFunct
    {
        public FileResult DuplicateAndOpenFile(string sessionId, string whichDoc);
        public IActionResult ShowPdf(string path);
        public string GetPath(string path);

        public List<MTIModel> GetMTIs();

        public List<AccountModel> GetAccounts();

        public List<StartWorkModel> GetStartWork();

        public List<PauseWorkModel> GetPauseWorks();

        public List<ProblemLogModel> GetProblemLogs();
    }

    public class UniversalFunctions : Controller, ISharedFunct
    {
        public UniversalFunctions(AppDbContext context)
        {
            _Db = context;
        }

        private readonly AppDbContext _Db;

        private readonly string userDir = "D:\\jtoledo\\Desktop\\DMD_SessionFolder";
        private readonly string mainDir = "D:\\jtoledo\\Desktop\\DocumentsHere\\";
        private readonly string tempDir = "D:\\jtoledo\\Desktop\\TempFiles";
        private readonly string userTravName = "Traveler.xlsx";
        private readonly string userLogName = "Logsheet.xlsx";

        private readonly string travName = "TravelerFileDoNotEdit.xlsx";
        private readonly string maindocName = "MainDoc.pdf";
        private readonly string assydrawingName = "AssyDrawing.pdf";
        private readonly string bomName = "BOM.pdf";
        private readonly string schemaName = "SchematicDiag.pdf";
        private readonly string oplName = "OPL.pdf";
        private readonly string prcoName = "PRCO.pdf";
        private readonly string derogationName = "Derogation.pdf";
        private readonly string memoName = "EngineeringMemo.pdf";

        public List<ProblemLogModel> GetProblemLogs()
        {
            return _Db.PLDb.ToList();
        }

        public List<MTIModel> GetMTIs()
        {
            return _Db.MTIDb.ToList();
        }

        public List<AccountModel> GetAccounts()
        {
            return _Db.AccountDb.ToList();
        }

        public List<StartWorkModel> GetStartWork()
        {
            return _Db.StartWorkDb.ToList();
        }

        public List<PauseWorkModel> GetPauseWorks()
        {
            return _Db.PauseWorkDb.ToList();
        }

        public string GetPath(string whichPath)
        {
            switch (whichPath)
            {
                case "mainDir":
                    {
                        return mainDir;
                    }
                case "userDir":
                    {
                        return userDir;
                    }
                case "tempDir":
                    {
                        return tempDir;
                    }
                case "userTravName":
                    {
                        return userTravName;
                    }
                case "travName":
                    {
                        return travName;
                    }
                case "logName":
                    {
                        return userLogName;
                    }
                default:
                    {
                        return "error";
                    }
            }
        }

        public FileResult DuplicateAndOpenFile(string sessionId, string whichDoc)
        {

            using (var package = new ExcelPackage(Path.Combine(userDir, sessionId, whichDoc)))
            {
                byte[] res = package.GetAsByteArray();
                return File(res, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", whichDoc);
            }

        }

        public IActionResult ShowPdf(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open);
            MemoryStream ms = new MemoryStream();
            fs.CopyTo(ms);

            return File(ms.ToArray(), "application/pdf");
        }
    }


}
