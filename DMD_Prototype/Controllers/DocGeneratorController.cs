using DMDLibrary;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.Reflection;

namespace DMD_Prototype.Controllers
{
    public class DocGeneratorController : Controller
    {
        private readonly ISharedFunct ishare;

        public DocGeneratorController(ISharedFunct ishare)
        {
            this.ishare = ishare;
        }

        public IActionResult DownloadExcel(string sessionId, string whichFile)
        {
            string filePath = Path.Combine(ishare.GetPath("userDir"), sessionId, ishare.GetPath(whichFile));

            return File(new COMHandler().DownloadExcel(filePath), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }

        public IActionResult DownloadTravelerTemplate(string docType)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            using (ExcelPackage package = new ExcelPackage(assembly.GetManifestResourceStream($"DMD_Prototype.wwwroot.Common.Templates.{docType}.xlsx")))
            {
                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return File(package.GetAsByteArray(), contentType, $"{docType.ToLower()}_Template.xlsx");
            }
        }

        public IActionResult ViewTraveler(string docNo)
        {
            string filePath = Path.Combine(ishare.GetPath("mainDir"), docNo, ishare.GetPath("travName"));
            string tempPath = $"{new COMHandler().GetAndConvertExcelFile(filePath)}.pdf";

            byte[] file = System.IO.File.ReadAllBytes(tempPath);

            System.IO.File.Delete(tempPath);

            return File(file, "application/pdf");
        }

        public IActionResult ViewExcelFile(string sessionId)
        {
            string filePath = Path.Combine(ishare.GetPath("userDir"), sessionId, ishare.GetPath("userTravName"));
            string tempPath = $"{new COMHandler().GetAndConvertExcelFile(filePath)}.pdf";

            byte[] file = System.IO.File.ReadAllBytes(tempPath);

            System.IO.File.Delete(tempPath);

            return File(file, "application/pdf");
        }

        public IActionResult DownloadPdf(string sessionId, string whichFile)
        {
            string filePath = Path.Combine(ishare.GetPath("userDir"), sessionId, ishare.GetPath(whichFile));
            string tempPath = $"{new COMHandler().GetAndConvertExcelFile(filePath)}.pdf";

            byte[] file = System.IO.File.ReadAllBytes(tempPath);

            System.IO.File.Delete(tempPath);

            return File(file, "application/pdf", $"{sessionId}.pdf");
        }

    }
}
