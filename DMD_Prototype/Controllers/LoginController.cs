using DMD_Prototype.Data;
using DMD_Prototype.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DMD_Prototype.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDbContext _Db;
        private readonly ISharedFunct ishared;

        public LoginController(AppDbContext _context, ISharedFunct ishared)
        {
            _Db = _context;
            this.ishared = ishared;
        }

        public IActionResult LoginPage()
        {

            string[]? loggedInUser = TempData["EN"] as string[];

            if (loggedInUser != null)
            {
                return RedirectToAction("Index", "Home");
            }
            
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Response.Cookies.Append("notifToast", "0");
            TempData.Clear();

            return View("LoginPage", "Login");
        }

        public IActionResult FailedLogin()
        {
           
            return View("Loginpage");
        }

        [HttpPost]
        public ContentResult TryLogin(string user, string pass)
        {
            AccountModel? acc = ishared.GetAccounts().FirstOrDefault(j => j.Username == user && j.Password == pass && !j.isDeleted);

            string jsonContent = string.Empty;
            string val = string.Empty;
            char type = 'c';

            if (acc != null)
            {
                
                string[] accData = { acc.AccName, acc.Role, acc.UserID };

                TempData["EN"] = null;
                TempData["EN"] = accData;

                if (CheckForActionSessionInSW(acc.UserID))
                {
                    val = $"?userID={acc.UserID}" + $"&noPW={true}";
                    type = 'a';
                }
                else if (ishared.GetRS().Any(j => j.UserId == acc.UserID))
                {
                    type = 'd';
                }
                else
                {
                    type = 'b';
                }
            }

            jsonContent = JsonConvert.SerializeObject(new { Type = type, nLink = val.ToString() });

            return Content(jsonContent, "application/json");
        }

        private bool CheckForActiveSession(string tech)
        {
            return ishared.GetPauseWorks().Any(j => j.Technician == tech && j.RestartDT == null);
        }

        private bool CheckForActionSessionInSW(string tech)
        {
            return ishared.GetStartWork().Any(j => j.UserID == tech && j.FinishDate == null);
        }

    }
}
