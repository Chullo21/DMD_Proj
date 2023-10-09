using DMD_Prototype.Data;
using Microsoft.AspNetCore.Mvc;
using DMD_Prototype.Models;
using OfficeOpenXml;
using Microsoft.CodeAnalysis;
using System.Linq;

namespace DMD_Prototype.Controllers
{
    public class MTIController : Controller
    {
        private readonly AppDbContext _Db;
        private readonly List<MTIModel> _mtiModel;
        private readonly List<AccountModel> _accounts;

        private readonly string mainDir = "D:\\jtoledo\\Desktop\\DocumentsHere\\";
        private readonly string usersDir = "D:\\jtoledo\\Desktop\\DMD_SessionFolder";

        private string DocumentNumberVar;

        private readonly string travName = "TravelerFileDoNotEdit.xlsx";
        private readonly string maindocName = "MainDoc.pdf";
        private readonly string assydrawingName = "AssyDrawing.pdf";
        private readonly string bomName = "BOM.pdf";
        private readonly string schemaName = "SchematicDiag.pdf";
        private readonly string oplName = "OPL.pdf";
        private readonly string prcoName = "PRCO.pdf";
        private readonly string derogationName = "Derogation.pdf";
        private readonly string memoName = "EngineeringMemo.pdf";

        public MTIController(AppDbContext _context)
        {
            _Db = _context;
            _mtiModel = _Db.MTIDb.ToList();
            _accounts = _Db.AccountDb.ToList();
        }

        public IActionResult EditDocument(string docuno, string assyno, string assydesc, string revno,
            IFormFile? mpti, IFormFile? bom, IFormFile? schema, IFormFile? drawing, List<IFormFile>? opl,
            List<IFormFile>? derogation, List<IFormFile>? prco, List<IFormFile>? memo, IFormFile? travFile,
            string logType, string? docctrlno, string? revnono)
        {

            var fromDb = _mtiModel.FirstOrDefault(j => j.DocumentNumber == docuno);

            bool changeLogType = fromDb.AfterTravLog != logType || (fromDb.LogsheetDocNo != docctrlno || fromDb.LogsheetRevNo != revnono);

            MTIModel mod = new MTIModel();
            {
                mod = fromDb;
                mod.AssemblyPN = assyno;
                mod.AssemblyDesc = assydesc;
                mod.RevNo = revno;
                mod.AfterTravLog = logType;
                mod.LogsheetDocNo = changeLogType ? docctrlno : mod.LogsheetDocNo;
                mod.LogsheetRevNo = changeLogType ? revnono : mod.LogsheetRevNo;
            }

            if (ModelState.IsValid)
            {
                DocumentNumberVar = docuno;
                CopyNoneMultipleDocs(mpti, drawing, bom, schema, travFile);
                CopyMultipleDocs(opl, prco, derogation, memo);

                _Db.MTIDb.Update(mod);
                _Db.SaveChanges();
            }

            return RedirectToAction("MTIView", new {docuNumber = docuno, workStat = false});
        }

        public IActionResult EditDocumentView(string docuNo)
        {
            MTIModel model = _mtiModel.FirstOrDefault(j => j.DocumentNumber == docuNo)!;

            return View(model);
        }

        public IActionResult MTIView(string docuNumber, bool workStat, string sesID, List<string> travelerProgress)
        {
            MTIModel mti = _mtiModel.FirstOrDefault(j => j.DocumentNumber == docuNumber);

            MTIViewModel mModel = new MTIViewModel();
            {
                mModel.DocumentNumber = docuNumber;
                mModel.Opl = DeviationDocNames(oplName, docuNumber);
                mModel.Prco = DeviationDocNames(prcoName, docuNumber);
                mModel.Derogation = DeviationDocNames(derogationName, docuNumber);
                mModel.Memo = DeviationDocNames(memoName, docuNumber);
                mModel.WorkingStat = workStat;
                mModel.SessionID = sesID;
                mModel.AssyNo = mti.AssemblyPN;
                mModel.AssyDesc = mti.AssemblyDesc;
                mModel.RevNo = mti.RevNo;
                mModel.AfterTravlog = mti.AfterTravLog;
                mModel.Product = mti.Product;
            }

            return View(mModel);
        }

        private List<string>? DeviationDocNames(string DocName, string docNo)
        {
            List<string>? listOfDocs = new List<string>();
            string folderPath = Path.Combine(mainDir, docNo);

            foreach (string docs in Directory.GetFiles(folderPath))
            {
                string FileNameOnly = Path.GetFileNameWithoutExtension(docs);
                
                if (docs.Contains(Path.GetFileNameWithoutExtension(DocName))) listOfDocs.Add(FileNameOnly);
            }

            return listOfDocs;
        }

        private byte[] getDocumentsFromDb(string docuNumber, string whichDoc, string extension)
        {
            string folderPath = Path.Combine(mainDir, docuNumber, whichDoc + extension);

            using (FileStream fileStream = new FileStream(folderPath, FileMode.Open))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    fileStream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }

        public IActionResult CreateMTIView()
        {
            return View();
        }

        public IActionResult CreateMTI(string documentnumber, string assynumber, string assydesc, string revnumber, 
            IFormFile? assemblydrawing, IFormFile? billsofmaterial, IFormFile? schematicdiagram, IFormFile mpti,
            List<IFormFile>? onepointlesson, List<IFormFile>? prco, List<IFormFile>? derogation, List<IFormFile>? engineeringmemo, 
            string product, string doctype, string originator, IFormFile TravelerFile, string afterTrav, string? docctrlno, string? revnono)
        {
            var fromDb = _mtiModel.FirstOrDefault(j => j.DocumentNumber == documentnumber);

            MTIModel mti = new MTIModel();
            {
                mti.DocumentNumber = documentnumber;
                mti.AssemblyPN = assynumber;
                mti.AssemblyDesc = assydesc;
                mti.RevNo = revnumber;
                mti.Product = product;
                mti.DocType = doctype;
                mti.OriginatorName = _accounts.FirstOrDefault(j => j.AccName == originator).UserID;
                mti.AfterTravLog = afterTrav;
                mti.LogsheetDocNo = docctrlno;
                mti.LogsheetRevNo = revnono;
            }

            if (ModelState.IsValid)
            {
                DocumentNumberVar = documentnumber;
                CreateNewFolder(documentnumber);
                CopyNoneMultipleDocs(mpti, assemblydrawing, billsofmaterial, schematicdiagram, TravelerFile);
                CopyMultipleDocs(onepointlesson, prco, derogation, engineeringmemo);

                _Db.MTIDb.Add(mti);
                _Db.SaveChanges();

                return RedirectToAction("MTIList", "Home", new { whichDoc = product });
            }
            else
            {
                TempData["Error"] = "Something went wrong, please try reuploading again.";
                return View("CreateMTIView", mti);
            }
        }

        private void CreateNewFolder(string docNumber)
        {
            Directory.CreateDirectory(Path.Combine(mainDir, docNumber));
        }

        private void CopyNoneMultipleDocs(IFormFile? mainDoc, IFormFile? AssyDrawing, IFormFile? BOM, IFormFile? Schematic, IFormFile? Traveler)
        {
            Dictionary<string, IFormFile> files = new Dictionary<string, IFormFile>();

            if (mainDoc != null) files.Add(maindocName, mainDoc);
            if (AssyDrawing != null) files.Add(assydrawingName, AssyDrawing);
            if (BOM  != null) files.Add(bomName, BOM);
            if (Schematic != null) files.Add(schemaName, Schematic);
            if (Traveler != null) files.Add(travName, Traveler);

            foreach (var file in files)
            {
                using (FileStream fs = new FileStream(Path.Combine(mainDir, DocumentNumberVar, file.Key), FileMode.Create))
                {
                    file.Value.CopyTo(fs);
                }
            }
        }

        private void CopyMultipleDocs(List<IFormFile>? onepointlesson, List<IFormFile>? prco, List<IFormFile>? derogation, List<IFormFile>? engineeringmemo)
        {         
            Dictionary<string, List<IFormFile>> files = new Dictionary<string, List<IFormFile>>();
            
            if (onepointlesson?.Count() > 0) files.Add(oplName, onepointlesson);
            if (prco?.Count() > 0) files.Add(prcoName, prco);
            if (derogation?.Count() > 0) files.Add(derogationName, derogation);
            if (engineeringmemo?.Count() > 0) files.Add(memoName, engineeringmemo);

            foreach (var file in files)
            {
                foreach (var item in file.Value)
                {
                    string filePath = Path.Combine(mainDir, DocumentNumberVar);

                    string fileNameOnly = Path.GetFileNameWithoutExtension(file.Key);
                    
                    List<string> getFiles = Directory.GetFiles(filePath).Where(j => j.Contains(fileNameOnly)).ToList();

                    using (FileStream fs = new FileStream(Path.Combine(filePath, fileNameOnly + "_" + (getFiles.Count + 1).ToString() + ".pdf"), FileMode.Create))
                    {
                        item.CopyTo(fs);
                    }
                }
            }
        }

        public IActionResult ShowDoc(string docunumber, string whichDoc)
        {
            if (whichDoc == "WS")
            {
                MemoryStream ms = new MemoryStream();
                FileStream fs = new FileStream(Path.Combine(mainDir, "1_WORKMANSHIP_STANDARD_FOLDER", "WS.pdf"), FileMode.Open);
                fs.CopyTo(ms);

                return File(ms.ToArray(), "application/pdf"); ;
            }

            byte[] res = null;

            List<string> files = Directory.GetFiles(Path.Combine(mainDir, docunumber)).ToList();

            if (Directory.GetFiles(Path.Combine(mainDir, docunumber)).Any(j => Path.GetFileName(j) == whichDoc + ".pdf"))
            {
                res = getDocumentsFromDb(docunumber, whichDoc, ".pdf");

                return File(res, "application/pdf");
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
        public string AfterTravlog { get; set; }
        public string Product { get; set; }

    }
}
