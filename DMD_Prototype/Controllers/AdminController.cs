using DMD_Prototype.Data;
using DMD_Prototype.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DMD_Prototype.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _Db;
        private readonly ISharedFunct ishare;
        public AdminController(AppDbContext dataBase, ISharedFunct shared)
        {
            _Db = dataBase;
            ishare = shared;
        }

        public ContentResult ShowTravelers()
        {
            string res = JsonConvert.SerializeObject(ishare.GetStartWork());
            return Content(res, "application/json");
        }

        public IActionResult AdminView()
        {
            return View(ishare.GetUA());
        }

        public IActionResult AccountsView()
        {
            return View(ishare.GetAccounts());
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

        public ContentResult GetObsoleteDocs()
        {
            List<MTIModel> mtis = ishare.GetMTIs().Where(j => j.ObsoleteStat).ToList();

            return Content(JsonConvert.SerializeObject(new { docs = mtis, check = mtis.Count}), "application/json");
        }

        public ContentResult DeleteObsoleteDocs(string adminName)
        {
            List<MTIModel> mtis = ishare.GetMTIs().Where(j => j.ObsoleteStat).ToList();

            string res = "bad";

            if (mtis.Count > 0)
            {
                res = "good";

                foreach (var mti in mtis)
                {
                    string directory = Path.Combine(ishare.GetPath("mainDir"), mti.DocumentNumber);
                    if (Directory.Exists(directory))
                    {
                        Directory.Delete(directory, true);
                    }

                    _Db.MTIDb.Remove(mti);
                }

                ishare.RecordOriginatorAction($"{adminName}, have cleared/deleted all obsolete documents.", adminName, DateTime.Now);
                _Db.SaveChanges();
            }           

            return Content(JsonConvert.SerializeObject(new {r = res}), "applicaiton/json");
        }

    }
}
