using DMD_Prototype.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using System.IO;

namespace DMD_Prototype.Controllers
{
    public class MTIController : Controller
    {
        private readonly AppDbContext _Db;

        public MTIController(AppDbContext _context)
        {
            _Db = _context;
        }
        public IActionResult MTIView()
        {
            return View();
        }

        public IActionResult CreateMTIView()
        {
            string folderPath = Path.Combine("D:", "jtoledo", "Desktop", "DocumentsHere");


            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            return View();
        }

        public IActionResult PNPViewDocs()
        {
            return View("MTIList");
        }

        public IActionResult CreateMTI()
        {

            return View();
        }

        public void CreateNewFolder()
        {
            
        }
    }
}
