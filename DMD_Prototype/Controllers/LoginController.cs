using DMD_Prototype.Data;
using DMD_Prototype.Models;
using Microsoft.AspNetCore.Mvc;

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

        public IActionResult TryLogin(string? user, string? pass)
        {
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass) || (string.IsNullOrEmpty(user) && string.IsNullOrEmpty(pass)))
            {
                TempData["LoginTemp"] = "Please fill both username and password";

                return RedirectToAction("FailedLogin");
            }
            else if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pass))
            {
                AccountModel? acc = _accounts.FirstOrDefault(j => j.Username == user && j.Password == pass);

                if (acc != null)
                {
                    string[] accData = { acc.AccName, acc.Role};

                    TempData["EN"] = null;

                    TempData["EN"] = accData;

                    if (CheckForActiveSession(acc.UserID))
                    {
                        return RedirectToAction("ContinueWork", "Work", new { userID = acc.UserID, noPW = false});
                    }
                    else if (CheckForActionSessionInSW(acc.UserID))
                    {
                        return RedirectToAction("ContinueWork", "Work", new { userID = acc.UserID, noPW = true});
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    
                }
                else
                {
                    TempData["LoginTemp"] = "Invalid username or password";

                    return RedirectToAction("FailedLogin");
                }
            }

            return RedirectToAction("Index", "Home");
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
