using DMD_Prototype.Data;
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

        public ContentResult SubmitAnnouncement(string message, string announcer)
        {
            if (ModelState.IsValid)
            {
                Db.AnnouncementDb.Add(new Models.AnnouncementModel().CreateAnnouncement(message, announcer));
                Db.SaveChanges();
            }

            return Content(JsonConvert.SerializeObject(new {response = "g"}), "application/json");
        }

        public ContentResult RemoveAnnouncement(int id)
        {
            if (ModelState.IsValid)
            {
                Db.AnnouncementDb.Remove(ishare.GetAnnouncements().FirstOrDefault(j => j.AnnouncementID == id));
                Db.SaveChanges();
            }

            return Content(JsonConvert.SerializeObject(new { response = "g" }), "application/json");
        }
    }
}
