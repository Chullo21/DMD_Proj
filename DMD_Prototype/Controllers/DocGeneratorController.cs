using DMD_Prototype.Data;
using DMD_Prototype.Models;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Controls;
using System.Reflection;

namespace DMD_Prototype.Controllers
{
    public class DocGeneratorController : Controller
    {
        private readonly string userDir = "D:\\jtoledo\\Desktop\\DMD_SessionFolder";       
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


        public IActionResult DownloadTraveler(string sessionId)
        {
            string filePath = Path.Combine(userDir, sessionId, userTravName);

            Response.Headers.Add("Content-Disposition", $"attachment; filename={sessionId}.xlsx");

            return File(new FileStream(filePath, FileMode.Open), "application/octet-stream");
        }

        
    }
}
