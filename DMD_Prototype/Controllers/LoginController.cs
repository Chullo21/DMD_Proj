using DMD_Prototype.Data;
using DMD_Prototype.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Json;

namespace DMD_Prototype.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDbContext _Db;
        private readonly List<AccountModel> _accounts;
        private readonly List<PauseWorkModel> _pauseWork;
        private readonly List<StartWorkModel> _swWork;

        public LoginController(AppDbContext _context)
        {
            _Db = _context;
            _accounts = _Db.AccountDb.ToList();
            _pauseWork = _Db.PauseWorkDb.ToList();
            _swWork = _Db.StartWorkDb.ToList();
        }

        public IActionResult LoginPage()
        {
            TempData.Clear();

            return View();
        }

        public IActionResult FailedLogin()
        {
            return View("Loginpage");
        }

        public ContentResult TryLogin(string user, string pass)
        {
            AccountModel? acc = _accounts.FirstOrDefault(j => j.Username == user && j.Password == pass);

            string jsonContent = string.Empty;
            string val = string.Empty;
            char type = 'c';

            if (acc != null)
            {
                string[] accData = { acc.AccName, acc.Role, acc.UserID };

                TempData["EN"] = null;

                TempData["EN"] = accData;

                if (CheckForActiveSession(acc.UserID))
                {
                    val = Path.Combine("Work", "ContinueWork") + $"?userID={acc.UserID}" + $"&noPW={false}";
                    type = 'a';
                }
                else if (CheckForActionSessionInSW(acc.UserID))
                {
                    val = Path.Combine("Work", "ContinueWork") + $"?userID={acc.UserID}" + $"&noPW={true}";
                    type = 'a';
                }
                else
                {
                    val = Path.Combine("Home", "Index");
                    type = 'a';
                }
            }

            jsonContent = JsonConvert.SerializeObject(new { Type = type, nLink = val });

            return Content(jsonContent, "application/json");
        }

        private bool CheckForActiveSession(string tech)
        {
            return _pauseWork.Any(j => j.Technician == tech && j.RestartDT == null);
        }

        private bool CheckForActionSessionInSW(string tech)
        {
            return _swWork.Any(j => j.UserID == tech && j.FinishDate == null);
        }

    }
}
