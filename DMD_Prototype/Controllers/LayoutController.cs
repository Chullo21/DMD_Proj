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

        private string isVisible = "0";

        public IActionResult ValidateUserSession()
        {
            bool isEmpty = true;

            string[]? userData = new string[3] {
                HttpContext.Session.GetString("AccountName") ?? "",
                HttpContext.Session.GetString("UserRole") ?? "",
                HttpContext.Session.GetString("UserID") ?? ""
            };

            if (userData != null)
            {
                isEmpty = false;
                ViewBag.EN = userData;
            }
        
            return Json(new { result = isEmpty });
        }

        public async Task<ContentResult> GetUISessionAsync()
        {
            if (HttpContext.Request.Cookies["notifToast"] != null)
            {
                isVisible = HttpContext.Request.Cookies["notifToast"].ToString();
            }
            else
            {
                HttpContext.Response.Cookies.Append("notifToast", isVisible);
            }

            var anns = await ishare.GetAnnouncements();

            foreach (var ann in anns)
            {
                string? accName = (await ishare.GetAccounts()).FirstOrDefault(j => j.UserID == ann.AnnouncementCreator)?.AccName;

                ann.AnnouncementCreator = string.IsNullOrEmpty(accName) ? "Account Deleted" : accName;
            }

            return Content(JsonConvert.SerializeObject(new {isVisible = isVisible, announcements = anns.ToArray()}), "application/json");
        }

        public ContentResult SetUISession(string isVisible)
        {
            HttpContext.Response.Cookies.Append("notifToast", isVisible);

            return Content(JsonConvert.SerializeObject(new { }), "application/json");
        }
    }
}
