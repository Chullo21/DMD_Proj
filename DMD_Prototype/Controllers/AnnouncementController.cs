using DMD_Prototype.Data;
using DMD_Prototype.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DMD_Prototype.Controllers
{
    public class AnnouncementController : Controller
    {
        private readonly AppDbContext Db;
        private readonly ISharedFunct ishare;

        public AnnouncementController(AppDbContext db, ISharedFunct ishare)
        {
            Db = db;
            this.ishare = ishare;
        }

        public async Task<IActionResult> SubmitAnnouncement(string message, string announcer)
        {
            if (ModelState.IsValid)
            {
                Db.AnnouncementDb.Add(new Models.AnnouncementModel().CreateAnnouncement(message, announcer));
                await Db.SaveChangesAsync();
            }

            return Json(new { response = "g" });
        }

        public async Task<IActionResult> RemoveAnnouncement(int id)
        {
            var announcement = await ishare.GetAnnouncements();

            AnnouncementModel ann = announcement.FirstOrDefault(j => j.AnnouncementID == id);

            if (ModelState.IsValid)
            {
                Db.AnnouncementDb.Remove(ann);
                await Db.SaveChangesAsync();
            }

            return Json(new { response = "g" });
        }
    }
}
