using DMD_Prototype.Data;
using DMD_Prototype.Models;
using Microsoft.AspNetCore.Mvc;

namespace DMD_Prototype.Controllers
{
    public class TravelerController : Controller
    {
        private readonly AppDbContext _Db;
        private readonly ISharedFunct ishared;

        public TravelerController(AppDbContext Db, ISharedFunct ishared)
        {
            _Db = Db;
            this.ishared = ishared;
        }

        public IActionResult ChangeTravWorker(int ID, string toWorker)
        {
            StartWorkModel sw = ishared.GetStartWork().FirstOrDefault(j => j.SWID == ID);
            sw.UserID = ishared.GetAccounts().FirstOrDefault(j => j.AccName == toWorker).UserID;

            if(ModelState.IsValid)
            {
                _Db.StartWorkDb.Update(sw);
                _Db.SaveChanges();
            }

            return RedirectToAction("ShowTravelers", "Home");
        }
    }
}
