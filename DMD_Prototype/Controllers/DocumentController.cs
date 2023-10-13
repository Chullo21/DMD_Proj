using DMD_Prototype.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DMD_Prototype.Controllers
{
    public class DocumentController : Controller
    {
        private readonly ISharedFunct _ishared;

        private readonly string userTravName = "Traveler.xlsx";
        private readonly string userLogName = "Logsheet.xlsx";

        public DocumentController(ISharedFunct ishared)
        {
            _ishared = ishared;
        }

        public IActionResult DownloadTraveler(string sessionId)
        {
            return _ishared.DuplicateAndOpenFile(sessionId, userTravName);
        }

        public IActionResult DownloadLogsheet(string sessionId)
        {
            return _ishared.DuplicateAndOpenFile(sessionId, userLogName);
        }
    }
}
