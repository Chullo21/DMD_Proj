using Microsoft.AspNetCore.Mvc;
using Microsoft.Office.Interop.Excel;
using OfficeOpenXml;
using System.Reflection;

namespace DMD_Prototype.Controllers
{
    public class DocGeneratorController : Controller
    {        
        public DocGeneratorController(ISharedFunct shared)
        {
            ishare = shared;
        }

        private readonly ISharedFunct ishare;

        private byte[] GetAndConvertExcelFile(string sessionId, string whichFile)
        {
            string srcDir = Path.Combine(ishare.GetPath("userDir"), sessionId, ishare.GetPath(whichFile));

            string outputDir = Path.Combine(ishare.GetPath("tempDir"), "PDF.pdf");

            Application excelApp = new Application();

            excelApp.Visible = false;

            Workbook workbook = excelApp.Workbooks.Open(srcDir);

            workbook.ExportAsFixedFormat(XlFixedFormatType.xlTypePDF, outputDir);

            workbook.Close(false);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
            excelApp.Quit();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);

            byte[] pdfInBytes = System.IO.File.ReadAllBytes(outputDir);

            System.IO.File.Delete(outputDir);

            return pdfInBytes;
        }

        public IActionResult DownloadExcel(string sessionId, string whichFile)
        {
            string filePath = Path.Combine(ishare.GetPath("userDir"), sessionId, ishare.GetPath(whichFile));

            using(ExcelPackage package = new ExcelPackage(filePath))
            {
                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return File(package.GetAsByteArray(), contentType, $"{sessionId}_{whichFile}.xlsx");
            }
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

        public IActionResult ViewExcelFile(string sessionId)
        {
            return File(GetAndConvertExcelFile(sessionId, "userTravName"), "application/pdf");
        }

        public IActionResult DownloadPdf(string sessionId, string whichFile)
        {
            return File(GetAndConvertExcelFile(sessionId, whichFile), "application/pdf", $"{sessionId}.pdf");
        }

    }
}
