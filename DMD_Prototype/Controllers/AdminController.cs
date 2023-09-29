using DMD_Prototype.Data;
using DMD_Prototype.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NuGet.Protocol;

namespace DMD_Prototype.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _Db;
        private readonly List<AccountModel> _accounts = new List<AccountModel>();
        private readonly List<StartWorkModel> _swModel;
        public AdminController(AppDbContext dataBase)
        {
            _Db = dataBase;
            _accounts = _Db.AccountDb.ToList();
            _swModel = _Db.StartWorkDb.ToList();
        }

        public ContentResult ShowTravelers()
        {
            string res = JsonConvert.SerializeObject(_swModel);
            return Content(res, "application/json");
        }

        public IActionResult AdminView()
        {
            return View(_accounts);
        }

        public IActionResult AccountsView()
        {
            return View(_accounts);
        }

        public IActionResult CreateAccount(string accname, string email, string sec, 
            string dom, string username, string password, string role)
        {

            if (ModelState.IsValid)
            {
                Guid newGuid = Guid.NewGuid();
                AccountModel createAcc = new AccountModel
                {
                    AccName = accname,
                    Email = email,
                    Sec = sec,
                    Dom = dom,
                    Username = username,
                    Password = password,
                    Role = role,
                    UserID = newGuid.ToString()[..10]
                };

                _Db.AccountDb.Add(createAcc);
                _Db.SaveChanges();
            }
            return RedirectToAction("AccountsView");
        }

        public IActionResult EditAccount(AccountModel account)
        {
            if (ModelState.IsValid)
            {
                var editAccount = _Db.AccountDb.FirstOrDefault(j => j.AccID == account.AccID);

                if (editAccount != null)
                {
                    editAccount.AccName = account.AccName;
                    editAccount.Email = account.Email;
                    editAccount.Sec = account.Sec;
                    editAccount.Dom = account.Dom;
                    editAccount.Username = account.Username;
                    editAccount.Password = account.Password;
                    editAccount.Role = account.Role;

                    _Db.AccountDb.Update(editAccount);
                    _Db.SaveChanges();
                }
            }

            return RedirectToAction("AccountsView");
        }

        public IActionResult DeleteAccount(int accid)
        {
            AccountModel? deleteAccount = _Db.AccountDb.FirstOrDefault(j => j.AccID == accid);

            if (deleteAccount != null)
            {
                _Db.AccountDb.Remove(deleteAccount);
                _Db.SaveChanges();
            }

            return RedirectToAction("AccountsView");
        }

    }
}
