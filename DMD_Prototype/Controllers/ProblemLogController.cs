using DMD_Prototype.Data;
using DMD_Prototype.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

namespace DMD_Prototype.Controllers
{
    public class ProblemLogController : Controller
    {
        private readonly AppDbContext _Db;
        private readonly List<ProblemLogModel> _plModel;
        private readonly List<AccountModel> _accounts;

        public ProblemLogController(AppDbContext context)
        {
            _Db = context;
            _plModel = _Db.PLDb.ToList();
            _accounts = _Db.AccountDb.ToList();
        }

        private string GetUsername()
        {
            string[] EN = TempData["EN"] as string[];
            TempData.Keep();

            return EN[0];
        }

        public IActionResult ProblemLogView()
        {
            List<ProblemLogModel> pls = new List<ProblemLogModel>();

            if (!_plModel.Any())
            {
                return View(pls);
            }

            foreach (var p in _plModel)
            {
                p.Owner = _accounts.FirstOrDefault(j => j.UserID == p.Owner).AccName;
                if (GetUsername() == p.Owner) pls.Add(p);
            }

            return View(pls);
        }

        public IActionResult SubmitPLValidation(ProblemLogModel fromView)
        {
            ProblemLogModel pl = _plModel.FirstOrDefault(j => j.PLID == fromView.PLID);
            {
                pl.OwnerRemarks = fromView.OwnerRemarks;
                pl.Category = fromView.Category;
                pl.RC = fromView.RC;
                pl.CA = fromView.CA;
                pl.InterimDoc = fromView.InterimDoc;
                pl.IDTCD = fromView.IDTCD;
                pl.IDStatus = fromView.IDStatus;
                pl.StandardizedDoc = fromView.StandardizedDoc;
                pl.SDTCD = fromView.SDTCD;
                pl.SDStatus = fromView.SDStatus;
                pl.Validation = fromView.Validation;
            }

            if (ModelState.IsValid)
            {
                _Db.PLDb.Update(pl);
                _Db.SaveChanges();
            }

            return View("ProblemLogView");
        }
    }
}
