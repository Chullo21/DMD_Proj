using DMDLibrary;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using PdfSharp.Drawing;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Security;
using PdfSharp.Pdf;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DMD_Prototype.Controllers
{
    public class DocGeneratorController : Controller
    {
        private readonly ISharedFunct ishare;

        public DocGeneratorController(ISharedFunct ishare)
        {
            this.ishare = ishare;
        }

        public async Task<IActionResult> DownloadExcel(string sessionId, string whichFile)
        {
            string filePath = Path.Combine(await ishare.GetPath("userDir"), sessionId, await ishare.GetPath(whichFile));

            return File(new COMHandler().DownloadExcel(filePath), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }

        public async Task<IActionResult> DownloadTravelerTemplate(string whichTraveler)
        {
            using (ExcelPackage package = new ExcelPackage(await ishare.GetPath(whichTraveler)))
            {
                if (package.Workbook.Worksheets.Count <= 0)
                {
                    return Ok("Template not found or does not exist");
                }

                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return File(package.GetAsByteArray(), contentType, $"{whichTraveler.ToLower()}_Template.xlsx");
            }
        }

        public async Task<IActionResult> ViewTraveler(string docNo)
        {
            string filePath = Path.Combine(await ishare.GetPath("mainDir"), docNo, await ishare.GetPath("travName"));
            string tempPath = new COMHandler().GetAndConvertExcelFile(filePath, await ishare.GetPath("tempDir"));

            byte[] file = System.IO.File.ReadAllBytes(tempPath);

            System.IO.File.Delete(tempPath);

            return File(file, "application/pdf");
        }

        public async Task<IActionResult> ViewExcelFile(string sessionId, string whichFile)
        {
            string filePath = Path.Combine(await ishare.GetPath("userDir"), sessionId, await ishare.GetPath(whichFile));

            if (!System.IO.File.Exists(filePath))
            { 
                return NoContent();
            }

            string tempPath = new COMHandler().GetAndConvertExcelFile(filePath, await ishare.GetPath("tempDir"));

            byte[] file = System.IO.File.ReadAllBytes(tempPath);

            System.IO.File.Delete(tempPath);

            return File(file, "application/pdf");
        }

        public async Task<IActionResult> DownloadFileWithFileName(string docNo, string fileName)
        {
            string filePath = Path.Combine(await ishare.GetPath("mainDir"), docNo, fileName);
            string tempPath = Path.Combine(await ishare.GetPath("tempDir"), Guid.NewGuid().ToString().Substring(0, 15) + ".pdf");

            System.IO.File.Copy(filePath, tempPath, true);

            AttachWatermarkInPdf(tempPath);
            byte[] file = System.IO.File.ReadAllBytes(tempPath);

            System.IO.File.Delete(tempPath);
            return File(file, "application/pdf", fileName);
        }

        public async Task<IActionResult> DownloadMainDoc(string docNo, string whichDoc)
        {
            string filePath = Path.Combine(await ishare.GetPath("mainDir"), docNo, await ishare.GetPath(whichDoc));

            if (!System.IO.File.Exists(filePath))
            {
                return NoContent();
            }

            string tempPath = Path.Combine(await ishare.GetPath("tempDir"), Guid.NewGuid().ToString().Substring(0, 15) + ".pdf");
            System.IO.File.Copy(filePath, tempPath, true);
            AttachWatermarkInPdf(tempPath);
            byte[] file = System.IO.File.ReadAllBytes(tempPath);

            System.IO.File.Delete(tempPath);
            return File(file, "application/pdf", Path.GetFileName(filePath));
        }

        public async Task<IActionResult> DownloadWS()
        {
            string filePath = Path.Combine(Path.Combine(await ishare.GetPath("mainDir"), await ishare.GetPath("wsf"), await ishare.GetPath("ws")));
            string tempPath = Path.Combine(await ishare.GetPath("tempDir"), Guid.NewGuid().ToString().Substring(0, 15) + ".pdf");
            System.IO.File.Copy(filePath, tempPath, true);
            AttachWatermarkInPdf(tempPath);
            byte[] file = System.IO.File.ReadAllBytes(tempPath);

            System.IO.File.Delete(tempPath);
            return File(file, "application/pdf", Path.GetFileName(filePath));
        }

        public async Task<IActionResult> DownloadPdf(string sessionId, string whichFile)
        {
            string filePath = Path.Combine(await ishare.GetPath("userDir"), sessionId, await ishare.GetPath(whichFile));
            string tempPath = new COMHandler().GetAndConvertExcelFile(filePath, await ishare.GetPath("tempDir"));

            AttachWatermarkInPdf(tempPath);
            byte[] file = System.IO.File.ReadAllBytes(tempPath);

            System.IO.File.Delete(tempPath);

            return File(file, "application/pdf", $"{whichFile}.pdf");
        }

        public void AttachWatermarkInPdf(string filePath)
        {

            if (filePath != "" || !string.IsNullOrEmpty(filePath))
            {
                PdfDocument inputDocument = PdfReader.Open(filePath, PdfDocumentOpenMode.Modify);

                if (inputDocument.Version < 14)
                {
                    inputDocument.Version = 14;
                }
                string strWM = "P. IMES CORP.";

                XFont fWatermark = new XFont("Arial", 100, XFontStyle.Bold);
                XFont fName = new XFont("Arial", 14, XFontStyle.Italic);

                int pageALL = inputDocument.PageCount;
                PdfPage page = inputDocument.Pages[0];

                XBrush penTextWM = new XSolidBrush(XColor.FromArgb(50, 0, 0, 0));
                XBrush penTextName = new XSolidBrush(XColor.FromArgb(200, 0, 0, 0));

                for (int i = 0; i < pageALL; i++)
                {
                    page = inputDocument.Pages[i];

                    var gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append);

                    // Get the size (in points) of the text.
                    var size = gfx.MeasureString(strWM, fWatermark);

                    gfx.DrawString(DateTime.Now.ToShortDateString(), fName, penTextName, new XRect(10, 5, 50, 20), XStringFormats.CenterLeft);
                    gfx.DrawString(DateTime.Now.ToShortDateString(), fName, penTextName, new XRect(page.Width - 170, page.Height - 25, 50, 20), XStringFormats.CenterLeft);

                    // Define a rotation transformation at the center of the page.
                    gfx.TranslateTransform(page.Width / 2, page.Height / 2);
                    gfx.RotateTransform(-Math.Atan(page.Height / page.Width) * 180 / Math.PI);
                    gfx.TranslateTransform(-page.Width / 2, -page.Height / 2);

                    // Create a string format.
                    var format = new XStringFormat();
                    format.Alignment = XStringAlignment.Near;
                    format.LineAlignment = XLineAlignment.Near;

                    // Draw the string.

                    gfx.DrawString(strWM, fWatermark, penTextWM, new XRect(0, 0, page.Width, page.Height), XStringFormats.Center);
                }

                inputDocument.Save(filePath);
            }
        }
    }
}
