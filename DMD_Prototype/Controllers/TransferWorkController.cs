using DMD_Prototype.Data;
using DMD_Prototype.Models;
using Microsoft.AspNetCore.Mvc;

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

        private List<SVSesViewModel> GetSVSes()
        {
            List<SVSesViewModel> res = new();

            Dictionary<string, string> docs = ishared.GetMTIs().Where(j => !j.ObsoleteStat).ToList().ToDictionary(j => j.DocumentNumber, j => j.AssemblyDesc);
            Dictionary<string, (string, string)> modules = ishared.GetModules().ToDictionary(j => j.SessionID, j => (j.Module, j.WorkOrder));
            IEnumerable<SerialNumberModel> serialNumbers = ishared.GetSerialNumbers();
            Dictionary<string, string> accounts = ishared.GetAccounts().Where(j => j.Role == "USER").ToList().ToDictionary(j => j.UserID, j => j.AccName);
            Dictionary<string, (string, string, string)> sw = ishared.GetStartWork().Where(j => j.FinishDate == null).ToList().ToDictionary(j => j.SWID.ToString(), j => (j.UserID, j.DocNo, j.SessionID));

            foreach (var s in sw)
            {
                SVSesViewModel vm = new();

                vm.DocNo = s.Value.Item2;
                vm.Desc = docs.FirstOrDefault(j => j.Key == s.Value.Item2).Value;

                string session = s.Value.Item3;
                vm.Module = modules.FirstOrDefault(j => j.Key == s.Value.Item3).Value.Item1;
                vm.SerialNo = modules.FirstOrDefault(j => j.Key == s.Value.Item3).Value.Item2;
                vm.WorkOrder = serialNumbers.FirstOrDefault(j => j.SessionId == s.Value.Item3).SerialNumber;

                vm.CurrentTech = accounts.FirstOrDefault(j => j.Key == s.Value.Item1).Value;

                vm.SWID = s.Key;

                res.Add(vm);
            }

            return res;

        }

        public IActionResult UserTWView()
        {
            return View(GetSVSes());
        }

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

            Dictionary<string, string> docs = ishared.GetMTIs().Where(j => !j.ObsoleteStat).ToList().ToDictionary(j => j.DocumentNumber, j => j.AssemblyDesc);
            Dictionary<string, (string, string)> modules = ishared.GetModules().ToDictionary(j => j.SessionID, j => (j.Module, j.WorkOrder));
            Dictionary<string, string> accounts = ishared.GetAccounts().Where(j => j.Role == "USER").ToList().ToDictionary(j => j.UserID, j => j.AccName);
            Dictionary<string, (string, string, string)> sw = ishared.GetStartWork().Where(j => j.FinishDate == null).ToList().ToDictionary(j => j.SWID.ToString(), j => (j.UserID, j.DocNo, j.SessionID));
            IEnumerable<SerialNumberModel> serialNumbers = ishared.GetSerialNumbers();
            List<SVSesViewModel> res = new();

            foreach (var req in reqs)
            {
                SVSesViewModel vm = new();
                vm.ReqId = req.TakeSessionID;
                vm.UserId = req.UserId;
                vm.SWID = req.SWID;
                vm.CurrentTech = accounts.FirstOrDefault(j => j.Key == sw.FirstOrDefault(j => j.Key == req.SWID).Value.Item1).Value;
                vm.Requestor = accounts.FirstOrDefault(j => j.Key == req.UserId).Value;
                vm.DocNo = sw.FirstOrDefault(j => j.Key == req.SWID).Value.Item2;
                vm.Desc = docs.FirstOrDefault(j => j.Key == sw.FirstOrDefault(j => j.Key == req.SWID).Value.Item2).Value;

                string sessionGetter = sw.FirstOrDefault(j => j.Key == req.SWID).Value.Item3;

                vm.Module = modules.FirstOrDefault(j => j.Key == sessionGetter).Value.Item1;
                vm.SerialNo = serialNumbers.FirstOrDefault(j => j.SessionId == sessionGetter).SerialNumber;
                vm.WorkOrder = modules.FirstOrDefault(j => j.Key == sessionGetter).Value.Item2;

                res.Add(vm);
            }

            return View(res);
        }

        public IActionResult ApproveSessionRequest(int RSID)
        {

            RequestSessionModel rs = ishared.GetRS().FirstOrDefault(j => j.TakeSessionID == RSID);

            CheckIfTechIsCurrentlyWorking(rs.UserId);

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

        public IActionResult TWPartial()
        {
            return PartialView("_TWPartial", GetSVSes());
        }

        public IActionResult RemoveSessionRequest(string RSID)
        {
            RequestSessionModel rsm = ishared.GetRS().FirstOrDefault(j => j.TakeSessionID == int.Parse(RSID));

            if (ModelState.IsValid)
            {
                _Db.RSDb.Remove(rsm);
                _Db.SaveChanges();
            }

            return RedirectToAction("SVTWView");
        }

        private void CheckIfTechIsCurrentlyWorking(string userId)
        {
            StartWorkModel sw = ishared.GetStartWork().FirstOrDefault(j => j.FinishDate == null && j.UserID == userId);

            if (sw != null)
            {
                sw.UserID = string.Empty;
                _Db.StartWorkDb.Update(sw);
            }
        }
    }

    public class SVSesViewModel
    {
        public int ReqId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string SWID { get; set; } = string.Empty;
        public string DocNo { get; set; } = string.Empty;
        public string Desc { get; set; } = string.Empty;
        public string WorkOrder { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public string SerialNo { get; set; } = string.Empty;
        public string CurrentTech { get; set; } = string.Empty;
        public string? Requestor { get; set; }
    }
}
