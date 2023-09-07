using DMD_Prototype.Data;
using DMD_Prototype.Models;
using Microsoft.AspNetCore.Mvc;

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
            return View();
        }

        public IActionResult ShowTraveler(IFormFile file)
        {
            MTIModel mti = new MTIModel();

            MemoryStream ms = new MemoryStream();
            file.CopyTo(ms);

            byte[] filearray = ms.ToArray();
            mti.Documnet1 = filearray;
            mti.Numberchuchu = "pewpew";

            _Db.MTIDb.Add(mti);
            _Db.SaveChanges();

            return View("MTIView", filearray);
        }

        public IActionResult ShowDoc()
        {
            MTIModel mti = _Db.MTIDb.Find(29);

            byte[] fileBytes = mti.Documnet1;

            return File(fileBytes, "application/pdf");
        }



        public class Bite
        {
            public byte[] Photo { get; set; }
        }
    }
}
