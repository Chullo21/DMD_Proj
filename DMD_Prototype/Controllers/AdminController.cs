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
        public readonly string connectionString = "This is a connection string";
        public AdminController(AppDbContext dataBase, ISharedFunct shared)
        {
            _Db = dataBase;
            ishare = shared;
        }

        public string GetConnectionString()
        {
            return connectionString;
        }

        public ContentResult ShowTravelers()
        {
            string res = JsonConvert.SerializeObject(ishare.GetStartWork());
            return Content(res, "application/json");
        }

        public async Task<IActionResult> AdminView()
        {
            return View(await ishare.GetUA());
        }

        public async Task<IActionResult> AccountsView()
        {
            return View((await ishare.GetAccounts()).Where(j => !j.isDeleted));
        }

        public async Task<IActionResult> CreateAccount(string accname, string email, string sec, 
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
                await _Db.SaveChangesAsync();
            }
            return RedirectToAction("AccountsView");
        }

        public async Task<IActionResult> EditAccount(AccountModel account)
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
                    await _Db.SaveChangesAsync();
                }
            }

            return RedirectToAction("AccountsView");
        }

        public async Task<IActionResult> DeleteAccount(int accid)
        {
            AccountModel? deleteAccount = _Db.AccountDb.FirstOrDefault(j => j.AccID == accid);

            if (deleteAccount != null)
            {
                deleteAccount.isDeleted = true;
                _Db.AccountDb.Update(deleteAccount);
                await _Db.SaveChangesAsync();
            }

            return RedirectToAction("AccountsView");
        }

        public async Task<ContentResult> GetObsoleteDocs()
        {
            List<MTIModel> mtis = (await ishare.GetMTIs()).Where(j => j.ObsoleteStat && !j.isDeleted).ToList();

            return Content(JsonConvert.SerializeObject(new { docs = mtis, check = mtis.Count}), "application/json");
        }

        public async Task<ContentResult> DeleteObsoleteDocs(string adminName)
        {
            List<MTIModel> mtis = (await ishare.GetMTIs()).Where(j => j.ObsoleteStat && !j.isDeleted).ToList();

            string res = "bad";

            if (mtis.Count > 0)
            {
                res = "good";

                foreach (var mti in mtis)
                {
                    string directory = Path.Combine(await ishare.GetPath("mainDir"), mti.DocumentNumber);
                    if (Directory.Exists(directory))
                    {
                        Directory.Delete(directory, true);
                    }

                    mti.isDeleted = true;

                    _Db.MTIDb.Update(mti);
                }

                await ishare.RecordOriginatorAction($"{adminName}, have cleared/deleted all obsolete documents.", adminName, DateTime.Now);
                await _Db.SaveChangesAsync();
            }           

            return Content(JsonConvert.SerializeObject(new {r = res}), "applicaiton/json");
        }

    }
}
