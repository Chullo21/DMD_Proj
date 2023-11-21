using DMD_Prototype.Data;
using DMD_Prototype.Data;
using Microsoft.AspNetCore.Mvc;
using DMD_Prototype.Models;
using OfficeOpenXml;
using Microsoft.CodeAnalysis;
using System.Linq;
using Newtonsoft.Json;

namespace DMD_Prototype.Controllers
{
    public class MTIController : Controller
    {
        public MTIController(ISharedFunct shared, AppDbContext db)
        {
            ishare = shared;
            _Db = db;
        }

        private readonly ISharedFunct ishare;
        private readonly AppDbContext _Db;

        private string DocumentNumberVar;

        public IActionResult ChangeDocOwner(string docNo, string  docOwner)
        {
            MTIModel mti = _Db.MTIDb.FirstOrDefault(j => j.DocumentNumber == docNo);
            {
                mti.OriginatorName = ishare.GetAccounts().FirstOrDefault(j => j.AccName == docOwner).UserID;
            }

            if (ModelState.IsValid)
            {
                _Db.MTIDb.Update(mti);
                _Db.SaveChanges();
            }

            return RedirectToAction("MTIList", "Home", new {whichDoc = mti.Product, whichType = mti.DocType});
        }

        public IActionResult DeleteDeviationDoc(string dir, string devType, string docNo)
        {
            System.IO.File.Delete(dir);

            string filePath = Path.Combine(ishare.GetPath("mainDir"), docNo);

            ViewData[devType] = Directory.GetFiles(filePath).Where(j => j.Contains(devType)).ToList();

            return Json(null);
        }

        public IActionResult EditDocumentDetails(MTIModel mti, string user)
        {
            MTIModel tempMTI = ishare.GetMTIs().FirstOrDefault(j => j.DocumentNumber == mti.DocumentNumber);

            tempMTI.AssemblyPN = mti.AssemblyPN;
            tempMTI.AssemblyDesc = mti.AssemblyDesc;
            tempMTI.RevNo = mti.RevNo;
            tempMTI.AfterTravLog = mti.AfterTravLog;
            tempMTI.LogsheetDocNo = mti.LogsheetDocNo;
            tempMTI.LogsheetRevNo = mti.LogsheetRevNo;
            tempMTI.ObsoleteStat = mti.ObsoleteStat;

            if (ModelState.IsValid)
            {
                string message = $"{user}, updated details of engineering Document with Doc Number of {mti.DocumentNumber}.";
                if (mti.ObsoleteStat) message = $"{user}, marked {mti.DocumentNumber} as obsolete.";

                ishare.RecordOriginatorAction(message, user, DateTime.Now);
                _Db.MTIDb.Update(tempMTI);
                _Db.SaveChanges();
            }

            return RedirectToAction("MTIList", "Home", new { whichDoc = tempMTI.Product, whichType = tempMTI.DocType});
        }

        public IActionResult EditDocument(string docuno,IFormFile? mpti, IFormFile? bom, IFormFile? schema, IFormFile? drawing, List<IFormFile>? opl,
            List<IFormFile>? derogation, List<IFormFile>? prco, List<IFormFile>? memo, IFormFile? travFile, List<string> DirsTobeDeleted, char mtpistatus,
            string user)
        {

            var fromDb = ishare.GetMTIs().FirstOrDefault(j => j.DocumentNumber == docuno);
            string docs = GetDocNames(mpti != null, drawing != null, bom != null, schema != null, opl.Count, prco.Count, derogation.Count, memo.Count);

            MTIModel mod = new MTIModel();
            {
                mod = fromDb;
                mod.MTPIStatus = mtpistatus;
            }

            if (ModelState.IsValid)
            {
                DocumentNumberVar = docuno;

                DeleteMultipleFiles(DirsTobeDeleted, docuno);
                CopyNoneMultipleDocs(mpti, drawing, bom, schema, travFile);
                CopyMultipleDocs(opl, prco, derogation, memo);

                _Db.MTIDb.Update(mod);

                ishare.RecordOriginatorAction($"{user}, updated the following engineering documents {docs} with Doc Number of {docuno}.", user, DateTime.Now);
                _Db.SaveChanges();
            }

            return RedirectToAction("MTIView", new {docuNumber = docuno, workStat = false});
        }

        public IActionResult EditDocumentView(string docuNo)
        {
            MTIModel model = ishare.GetMTIs().FirstOrDefault(j => j.DocumentNumber == docuNo)!;

            string filePath = Path.Combine(ishare.GetPath("mainDir"), docuNo);

            ViewBag.opl= Directory.GetFiles(filePath).Where(j => j.Contains("OPL")).ToList();
            ViewBag.derogation = Directory.GetFiles(filePath).Where(j => j.Contains("Derogation")).ToList();
            ViewBag.prco = Directory.GetFiles(filePath).Where(j => j.Contains("PRCO")).ToList();
            ViewBag.memo = Directory.GetFiles(filePath).Where(j => j.Contains("EngineeringMemo")).ToList();

            return View(model);
        }

        public IActionResult MTIView(string docuNumber, bool workStat, string sesID)
        {
            MTIModel mti = ishare.GetMTIs().FirstOrDefault(j => j.DocumentNumber == docuNumber);

            MTIViewModel mModel = new MTIViewModel();
            {
                mModel.DocumentNumber = docuNumber;
                mModel.Opl = DeviationDocNames(ishare.GetPath("opl"), docuNumber);
                mModel.Prco = DeviationDocNames(ishare.GetPath("prco"), docuNumber);
                mModel.Derogation = DeviationDocNames(ishare.GetPath("derog"), docuNumber);
                mModel.Memo = DeviationDocNames(ishare.GetPath("memo"), docuNumber);
                mModel.WorkingStat = workStat;
                mModel.SessionID = sesID;
                mModel.AssyNo = mti.AssemblyPN;
                mModel.AssyDesc = mti.AssemblyDesc;
                mModel.RevNo = mti.RevNo;
                mModel.AfterTravlog = mti.AfterTravLog;
                mModel.Product = mti.Product;
                mModel.DocType = mti.DocType;
                mModel.ObsoleteStat = mti.ObsoleteStat;
            }

            return View(mModel);
        }

        public ContentResult ValidateDocNo(string DocNo)
        {
            string jsonResponse = "";

            if (ishare.GetMTIs().Any(j => j.DocumentNumber == DocNo)) jsonResponse = "Document Number already exist\r\n";

            return Content(JsonConvert.SerializeObject(new {Failed = jsonResponse}), "application/json");
        }

        private void DeleteMultipleFiles(List<string> fileNames, string docNo)
        {
            foreach (string filename in fileNames)
            {
                System.IO.File.Delete(Path.Combine(ishare.GetPath("mainDir"), docNo, $"{filename}.pdf"));
            }
        }

        private string GetDocNames(bool mpti, bool drawing, bool bom, bool schema, int opl, int prco, int derogation, int memo)
        {
            string res = "";
            List<string> listRes = new List<string>();

            if (mpti) listRes.Add("MPI/MTI");
            if (drawing) listRes.Add("Assembly Drawing");
            if (bom) listRes.Add("BOM");
            if (schema) listRes.Add("Schematic Diagram");
            if (opl > 0) listRes.Add("OPL");
            if (prco > 0) listRes.Add("PRCO");
            if (derogation > 0) listRes.Add("Derogation");
            if (memo > 0) listRes.Add("Engineering Memo");

            foreach (string entry in listRes)
            {
                if (res.EndsWith(',')) res += $" {entry},"; else res += entry + ",";
            }

            if (res.EndsWith(',')) res = res.Substring(0, res.Length - 1);

            return res;
        }

        private List<string>? DeviationDocNames(string DocName, string docNo)
        {
            List<string>? listOfDocs = new List<string>();
            string folderPath = Path.Combine(ishare.GetPath("mainDir"), docNo);

            foreach (string docs in Directory.GetFiles(folderPath))
            {
                string FileNameOnly = Path.GetFileNameWithoutExtension(docs);
                
                if (docs.Contains(Path.GetFileNameWithoutExtension(DocName))) listOfDocs.Add(FileNameOnly);
            }

            return listOfDocs;
        }

        public IActionResult CreateMTIView()
        {
            return View();
        }

        public IActionResult CreateMTI(string documentnumber, string assynumber, string assydesc, string revnumber, 
            IFormFile? assemblydrawing, IFormFile? billsofmaterial, IFormFile? schematicdiagram, IFormFile mpti,
            List<IFormFile>? onepointlesson, List<IFormFile>? prco, List<IFormFile>? derogation, List<IFormFile>? engineeringmemo, 
            string product, string doctype, string originator, IFormFile TravelerFile, string afterTrav, string? logsheetDocNo, string? logsheetRevNo)
        {

            MTIModel mti = new MTIModel();
            {
                mti.DocumentNumber = documentnumber;
                mti.AssemblyPN = assynumber;
                mti.AssemblyDesc = assydesc;
                mti.RevNo = revnumber;
                mti.Product = product;
                mti.DocType = doctype;
                mti.OriginatorName = ishare.GetAccounts().FirstOrDefault(j => j.AccName == originator).UserID;
                mti.AfterTravLog = afterTrav;
                mti.LogsheetDocNo = logsheetDocNo;
                mti.LogsheetRevNo = logsheetRevNo;
            }

            if (ModelState.IsValid)
            {
                DocumentNumberVar = documentnumber;

                CreateNewFolder(documentnumber);
                CopyNoneMultipleDocs(mpti, assemblydrawing, billsofmaterial, schematicdiagram, TravelerFile);
                CopyMultipleDocs(onepointlesson, prco, derogation, engineeringmemo);
                ishare.RecordOriginatorAction($"{originator}, uploaded engineering doc/s with Doc Number of {documentnumber}.", originator, DateTime.Now);

                _Db.MTIDb.Add(mti);
                _Db.SaveChanges();
            }

            return RedirectToAction("MTIList", "Home", new { whichDoc = product });
        }

        private void CreateNewFolder(string docNumber)
        {
            Directory.CreateDirectory(Path.Combine(ishare.GetPath("mainDir"), docNumber));
        }

        private void CopyNoneMultipleDocs(IFormFile? mainDoc, IFormFile? AssyDrawing, IFormFile? BOM, IFormFile? Schematic, IFormFile? Traveler)
        {
            Dictionary<string, IFormFile> files = new Dictionary<string, IFormFile>();

            if (mainDoc != null) files.Add(ishare.GetPath("mainDoc"), mainDoc);
            if (AssyDrawing != null) files.Add(ishare.GetPath("assy"), AssyDrawing);
            if (BOM  != null) files.Add(ishare.GetPath("bom"), BOM);
            if (Schematic != null) files.Add(ishare.GetPath("schema"), Schematic);
            if (Traveler != null) files.Add(ishare.GetPath("travName"), Traveler);

            foreach (var file in files)
            {
                using (FileStream fs = new FileStream(Path.Combine(ishare.GetPath("mainDir"), DocumentNumberVar, file.Key), FileMode.Create))
                {
                    file.Value.CopyTo(fs);
                }
            }
        }

        private void CopyMultipleDocs(List<IFormFile>? onepointlesson, List<IFormFile>? prco, List<IFormFile>? derogation, List<IFormFile>? engineeringmemo)
        {
            Dictionary<string, List<IFormFile>> files = new Dictionary<string, List<IFormFile>>();

            if (onepointlesson?.Count > 0) files.Add(ishare.GetPath("opl"), onepointlesson);
            if (prco?.Count > 0) files.Add(ishare.GetPath("prco"), prco);
            if (derogation?.Count > 0) files.Add(ishare.GetPath("derog"), derogation);
            if (engineeringmemo?.Count > 0) files.Add(ishare.GetPath("memo"), engineeringmemo);

            string filePath = Path.Combine(ishare.GetPath("mainDir"), DocumentNumberVar);

            foreach (var file in files)
            {
                foreach (var item in file.Value)
                {
                    int counter = 1;

                    string fileNameOnly = Path.GetFileNameWithoutExtension(file.Key);

                    List<string> getFiles = Directory.GetFiles(filePath).Where(j => j.Contains(fileNameOnly)).ToList();

                    do
                    {
                        if (!System.IO.File.Exists(Path.Combine(filePath, $"{fileNameOnly}_{counter}.pdf")))
                        {
                            using (FileStream fs = new FileStream(Path.Combine(filePath, fileNameOnly + "_" + (getFiles.Count + 1).ToString() + ".pdf"), FileMode.Create))
                            {
                                item.CopyTo(fs);
                            }

                            break;
                        }
                        counter++;
                    } while (true);
                }
            }
        }

        private byte[] getDocumentsFromDb(string docuNumber, string whichDoc, string extension)
        {
            string folderPath = Path.Combine(ishare.GetPath("mainDir"), docuNumber, whichDoc + extension);

            using (FileStream fileStream = new FileStream(folderPath, FileMode.Open))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    fileStream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }

        public IActionResult UploadWS(IFormFile file)
        {
            using (FileStream fs = new FileStream(Path.Combine(ishare.GetPath("mainDir"), ishare.GetPath("wsf"), ishare.GetPath("ws")), FileMode.Create))
            {
                file.CopyTo(fs);
            }

            return RedirectToAction("Index", "Home");
        }

        public IActionResult ShowDoc(string docunumber, string whichDoc)
        {
            if (whichDoc == "WS")
            {
                if (System.IO.File.Exists(Path.Combine(ishare.GetPath("mainDir"), ishare.GetPath("wsf"), ishare.GetPath("ws"))))
                {
                    using (FileStream fs = new FileStream(Path.Combine(ishare.GetPath("mainDir"), ishare.GetPath("wsf"), ishare.GetPath("ws")), FileMode.Open))
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            fs.CopyTo(ms);
                            return File(ms.ToArray(), "application/pdf");
                        }
                    }
                }
                else
                {
                    return NoContent();
                }
                          
            }
            else if (Directory.GetFiles(Path.Combine(ishare.GetPath("mainDir"), docunumber)).Any(j => Path.GetFileName(j) == whichDoc + ".pdf"))
            {
                return File(getDocumentsFromDb(docunumber, whichDoc, ".pdf"), "application/pdf");
            }
            else
            {
                return NoContent();
            }          
        }
    }

    class NameSetter
    {
        public IFormFile Doc { get; set; }
        public string Name { get; set; }

        public NameSetter(IFormFile doc, string name)
        {
            this.Doc = doc;
            this.Name = name;
        }
    }

    public class MTIViewModel
    {
        public string DocumentNumber { get; set; } = string.Empty;
        public string AssyNo { get; set; } = string.Empty;
        public string AssyDesc { get; set; } = string.Empty;
        public string RevNo { get; set; } = string.Empty;
        public List<string>? Opl { get; set; }
        public List<string>? Prco { get; set; }
        public List<string>? Derogation { get; set; }
        public List<string>? Memo { get; set; }
        public bool WorkingStat { get; set; } = false;
        public string? SessionID { get; set; }
        public string AfterTravlog { get; set; } = string.Empty;
        public string Product { get; set; } = string.Empty;
        public string DocType { get; set; } = string.Empty;
        public bool ObsoleteStat { get; set; } = false;

    }

    public class CreateMPTIModel
    {
        public string DocumentNumber { get; set; } = string.Empty;
        public string AssyNo { get; set; } = string.Empty;
        public string AssyDesc { get; set; } = string.Empty;
        public string RevNo { get; set; } = string.Empty;
        public IFormFile? MPTI { get; set; }
        public IFormFile? Traveler { get; set; }
        public IFormFile? BOM { get; set; }
        public IFormFile? Drawing { get; set; }
        public IFormFile? Schema { get; set; }
        public string? Message { get; set; }
    }
}
