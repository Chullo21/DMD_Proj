using DMD_Prototype.Data;
using DMD_Prototype.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Utils;
using System.Diagnostics;
using System.Reflection;
using System.Security.AccessControl;

namespace DMD_Prototype.Controllers
{
    public class DocGeneratorController : Controller
    {        
        public DocGeneratorController(ISharedFunct shared)
        {
            _shared = shared;
        }

        private readonly ISharedFunct _shared;

        public void OpenExcelFile(string sessionId, string user)
        {
            string theFile = Path.Combine(_shared.GetPath("userDir"), sessionId, _shared.GetPath("userTravName"));

            string tempPath = _shared.GetPath("tempDir");

            Random rand = new Random();

            string procPath = Path.Combine(tempPath, $"{rand.Next()}.xlsx");

            do
            {
                procPath = Path.Combine(tempPath, $"{rand.Next()}.xlsx");
            } while (System.IO.File.Exists(procPath));

            ExcelPackage package = new ExcelPackage(theFile);

            package.SaveAs(procPath);

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = procPath,
                UseShellExecute = true
            };

            Process.Start(startInfo);

        }


    }
}
