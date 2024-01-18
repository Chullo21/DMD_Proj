using DMD_Prototype.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DMD_Prototype.Controllers
{
    public class LayoutController : Controller
    {
        private readonly ISharedFunct ishare;

        public LayoutController(ISharedFunct ishare)
        {
            this.ishare = ishare;
        }

        private string isVisible = "1";

        public ContentResult GetUISession()
        {
            if (HttpContext.Request.Cookies["notifToast"] != null)
            {
                isVisible = HttpContext.Request.Cookies["notifToast"].ToString();
            }

            List<AnnouncementModel> anns = ishare.GetAnnouncements().ToList();

            foreach (var ann in anns)
            {
                string? accName = ishare.GetAccounts().FirstOrDefault(j => j.UserID == ann.AnnouncementCreator)?.AccName;

                ann.AnnouncementCreator = string.IsNullOrEmpty(accName) ? "Account Deleted" : accName;
            }

            return Content(JsonConvert.SerializeObject(new {isVisible = isVisible, announcements = ishare.GetAnnouncements().ToArray()}), "application/json");
        }

        public ContentResult SetUISession(string isVisible)
        {
            HttpContext.Response.Cookies.Append("notifToast", isVisible);

            return Content(JsonConvert.SerializeObject(new { }), "application/json");
        }
    }
}
