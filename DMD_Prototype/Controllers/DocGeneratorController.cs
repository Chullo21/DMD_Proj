using DMD_Prototype.Data;
using DMD_Prototype.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Reflection;

namespace DMD_Prototype.Controllers
{
    public class DocGeneratorController : Controller
    {
        private readonly string userDir = "D:\\jtoledo\\Desktop\\DMD_SessionFolder";
        private readonly string autoDir = "D:\\jtoledo\\Desktop\\SystemAutomations";
        private readonly string userTravName = "Traveler.xlsx";

        private readonly AppDbContext _Db;
        private readonly List<MTIModel> _mtiModel;
        private readonly List<StartWorkModel> _swModel;

        public DocGeneratorController(AppDbContext _context)
        {
            _Db = _context;
            _mtiModel = _Db.MTIDb.ToList();
            _swModel = _Db.StartWorkDb.ToList();
        }

        public void ConvertExcelToPdf(string sessionId)
        {
            
            string resourceName = "DMD_Prototype.wwwroot.Common.Automations.Excel_To_PDF_Converter.bat";

            string tempBatchFilePath = Path.Combine(Path.GetTempPath(), "Excel_To_PDF_Converter.bat");

            string from = Path.Combine(userDir, sessionId, userTravName);
            string to = Path.Combine(userDir, sessionId);

            using (Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            using (FileStream fileStream = new FileStream(tempBatchFilePath, FileMode.Create))
            {
                resourceStream.CopyTo(fileStream);
            }

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(tempBatchFilePath),
                Arguments = $"/K \"{tempBatchFilePath}\" {from} {to}"
            };

            using (Process process = new Process { StartInfo = startInfo })
            {
                process.Start();
                process.WaitForExit();
            }

        }

    }
}
