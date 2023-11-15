using DMD_Prototype.Data;
using DMD_Prototype.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DMD_Prototype.Controllers
{
    public class TransferWorkController : Controller
    {
        private readonly ISharedFunct ishared;
        private readonly AppDbContext _Db;

        public TransferWorkController (ISharedFunct ishared, AppDbContext _context)
        {
            this.ishared = ishared;
            _Db = _context;
        }

        public IActionResult UserTWView()
        {
            return View(ishared.GetStartWork().Where(j => j.FinishDate == null).ToList());
        }

        //public ContentResult UpdateSessions()
        //{
        //    List<StartWorkModel> sessions = ishared.GetStartWork().Where(j => j.FinishDate == null).ToList();

        //    return Content(JsonConvert.SerializeObject(new {r = sessions}), "application/json");
        //}

        public IActionResult TakeSession(string id, string userId)
        {
            if (ModelState.IsValid)
            {
                _Db.RSDb.Add(new RequestSessionModel().CreateSessionRequest(userId, id));
                _Db.SaveChanges();
            }

            return RedirectToAction("LoginPage", "Login");
        }

        public IActionResult SVTWView()
        {

            List<RequestSessionModel> reqs = ishared.GetRS();

            Dictionary<string, string> accounts = ishared.GetAccounts().Where(j => j.Role == "USER").ToList().ToDictionary(j => j.UserID, j => j.AccName);
            Dictionary<string, string> sw = ishared.GetStartWork().Where(j => j.FinishDate == null).ToList().ToDictionary(j => j.SWID.ToString(), j => j.UserID);

            List<SVSesViewModel> res = new();

            foreach (var req in reqs)
            {
                SVSesViewModel vm = new();
                vm.ReqId = req.TakeSessionID;
                vm.UserId = req.UserId;
                vm.SWID = req.SWID;
                vm.CurrentTech = accounts.FirstOrDefault(j => j.Key == sw.FirstOrDefault(j => j.Key == req.SWID).Value).Value;
                vm.Requestor = accounts.FirstOrDefault(j => j.Key == req.UserId).Value;

                res.Add(vm);
            }

            return View(res);
        }

        //public ContentResult UpdateRequests()
        //{
        //    List<RequestSessionModel> reqs = ishared.GetRS();

        //    Dictionary<string, string> accounts = ishared.GetAccounts().Where(j => j.Role == "USER").ToList().ToDictionary(j => j.UserID, j => j.AccName);
        //    Dictionary<string, string> sw = ishared.GetStartWork().Where(j => j.FinishDate == null).ToList().ToDictionary(j => j.SWID.ToString(), j => j.UserID);

        //    List<SVSesViewModel> res = new();

        //    foreach(var req in reqs)
        //    {
        //        SVSesViewModel vm = new();
        //        vm.ReqId = req.TakeSessionID;
        //        vm.UserId = req.UserId;
        //        vm.SWID = req.SWID;
        //        vm.CurrentTech = accounts.FirstOrDefault(j => j.Key == sw.FirstOrDefault(j => j.Key == req.SWID).Value).Value;
        //        vm.Requestor = accounts.FirstOrDefault(j => j.Key == req.UserId).Value;

        //        res.Add(vm);
        //    }

        //    return Content(JsonConvert.SerializeObject(new {r = res}), "application/json");
        //}

        public IActionResult ApproveSessionRequest(int RSID)
        {
            RequestSessionModel rs = ishared.GetRS().FirstOrDefault(j => j.TakeSessionID == RSID);

            List<RequestSessionModel> rsList = ishared.GetRS().Where(j => j.SWID == rs.SWID).ToList();

            StartWorkModel sw = ishared.GetStartWork().FirstOrDefault(j => j.SWID == int.Parse(rs.SWID));

            sw.UserID = rs.UserId;

            if (ModelState.IsValid)
            {
                _Db.StartWorkDb.Update(sw);
                _Db.RSDb.Remove(rs);

                foreach(var req in rsList)
                {
                    _Db.RSDb.Remove(req);
                }

                _Db.SaveChanges();
            }

            return RedirectToAction("SVTWView", "TransferWork");
        }
    }

    public class SVSesViewModel
    {
        public int ReqId { get; set; }
        public string UserId { get; set; }
        public string SWID { get; set; }
        public string CurrentTech { get; set; }
        public string Requestor { get; set; }
    }
}
