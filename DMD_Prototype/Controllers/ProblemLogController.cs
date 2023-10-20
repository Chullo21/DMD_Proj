using DMD_Prototype.Data;
using DMD_Prototype.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace DMD_Prototype.Controllers
{
    public class ProblemLogController : Controller
    {
        private readonly AppDbContext _Db;
        private readonly ISharedFunct ishare;

        private readonly List<ProblemLogModel> _plModel;
        private readonly List<AccountModel> _accounts;

        public ProblemLogController(AppDbContext context, ISharedFunct shared)
        {
            _Db = context;
            _plModel = _Db.PLDb.ToList();
            _accounts = _Db.AccountDb.ToList();
            ishare = shared;
        }

        private string[] GetUsername()
        {
            string[] EN = TempData["EN"] as string[];
            TempData.Keep();

            return EN;
        }

        public IActionResult PLSubmitValidation(int plID, string Validator, string PLIDStatus, string PLSDStatus, string PLRemarks)
        {           
            ProblemLogModel pl = _plModel.FirstOrDefault(j => j.PLID == plID);

            switch (PLIDStatus)
            {
                case "OPEN":
                    {
                        pl.IDStatus = "DENIED";
                        break;
                    }
                case "CLOSEd":
                    {
                        pl.IDStatus = "CLOSED";
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            switch (PLSDStatus)
            {
                case "OPEN":
                    {
                        pl.SDStatus = "DENIED";
                        break;
                    }
                case "CLOSEd":
                    {
                        pl.SDStatus = "CLOSED";
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            pl.Validator = Validator;
            pl.PLIDStatus = PLIDStatus;
            pl.PLSDStatus = PLSDStatus;
            pl.PLRemarks = PLRemarks;

            if (ModelState.IsValid)
            {
                _Db.PLDb.Update(pl);
                _Db.SaveChanges();
            }

            return RedirectToAction("ProblemLogView");
        }

        public IActionResult EditPLValidation(int plid, string rc, string ca, string interimdoc, string standardizeddoc)
        {
            ProblemLogModel pl = _plModel.FirstOrDefault(j => j.PLID == plid);

            pl.RC = rc;
            pl.CA = ca;
            pl.InterimDoc = interimdoc;
            pl.StandardizedDoc = standardizeddoc;

            if (ModelState.IsValid)
            {
                _Db.PLDb.Update(pl);
                _Db.SaveChanges();
            }

            return RedirectToAction("ProblemLogView");
        }

        public IActionResult ProblemLogView()
        {
            List<ProblemLogViewModel> pls = new List<ProblemLogViewModel>();

            if (!_plModel.Any())
            {
                return View(pls);
            }

            foreach (var p in _plModel)
            {
                ProblemLogViewModel mod = new ProblemLogViewModel();
                mod.Owner = ishare.GetMTIs().FirstOrDefault(j => j.DocumentNumber == p.DocNo).OriginatorName;
                mod.PL = p;

                pls.Add(mod);
            }

            return View(pls.OrderByDescending(j => j.PL.LogDate));
        }

        public IActionResult SubmitPLValidation(ProblemLogModel fromView)
        {
            string sdVal;

            if (fromView.Validation == "Invalid")
            {
                sdVal = "";
            }
            else
            {
                sdVal = string.IsNullOrEmpty(fromView.StandardizedDoc) ? "No input" : fromView.StandardizedDoc;
            }

            ProblemLogModel pl = _plModel.FirstOrDefault(j => j.PLID == fromView.PLID);
            {
                pl.OwnerRemarks = fromView.OwnerRemarks;
                pl.Category = fromView.Category;
                pl.RC = fromView.RC;
                pl.CA = fromView.CA;
                pl.InterimDoc = fromView.InterimDoc;
                pl.IDTCD = fromView.IDTCD;
                pl.IDStatus = fromView.Validation == "Valid" ? "For Validation" : "";
                pl.StandardizedDoc = sdVal;
                pl.SDTCD = fromView.SDTCD;
                pl.SDStatus = fromView.Validation == "Valid" ? "OPEN" : "";
                pl.Validation = fromView.Validation;
            }

            if (ModelState.IsValid)
            {
                _Db.PLDb.Update(pl);
                _Db.SaveChanges();
            }

            return RedirectToAction("ProblemLogView");
        }

        public ContentResult GetPLTCDDates(string val)
        {
            DateTime currentDate = DateTime.Now;

            DateTime ID;
            switch (val)
            {
                case "A":
                    {
                        ID = GetWorkingDays(1);
                        break;
                    }
                case "B":
                    {
                        ID = GetWorkingDays(2);
                        break;
                    }
                case "C":
                    {
                        ID = GetWorkingDays(5);
                        break;
                    }
                default:
                    {
                        ID = GetWorkingDays(1);
                        break;
                    }
            }

            DateTime SD = new DateTime(currentDate.Year, currentDate.AddMonths(1).Month, 10);

            string jsonContent = JsonConvert.SerializeObject(new {ID = ID.ToString("yyyy-MM-dd"), SD = SD.ToString("yyyy-MM-dd") });

            return Content(jsonContent, "application/json");
        }

        private DateTime GetWorkingDays(int days)
        {
            DateTime res = DateTime.Now;
            int counter = 1;

            do
            {
                res = res.AddDays(1);
                if (res.DayOfWeek != DayOfWeek.Sunday && res.DayOfWeek != DayOfWeek.Saturday)
                {
                    counter++;
                }

            } while (counter <= days);

            return res;
        }
    }

    public class ProblemLogViewModel
    {
        public ProblemLogModel PL { get; set; }
        public string Owner { get; set; }
    }
}
