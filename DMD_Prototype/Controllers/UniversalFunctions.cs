using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

namespace DMD_Prototype.Controllers
{   
    public interface ISharedFunct
    {
        public FileResult DuplicateAndOpenFile(string sessionId, string whichDoc);
        public IActionResult ShowPdf(string path);
    }

    public class UniversalFunctions : Controller, ISharedFunct
    {
        private readonly string userDir = "D:\\jtoledo\\Desktop\\DMD_SessionFolder";
        private readonly string userTravName = "Traveler.xlsx";
        private readonly string userLogName = "Logsheet.xlsx";

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
