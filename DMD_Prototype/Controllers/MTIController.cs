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
            ishared = shared;
            _Db = db;
        }

        private readonly ISharedFunct ishared;
        private readonly AppDbContext _Db;

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

        public IActionResult ChangeDocOwner(string docNo, string  docOwner)
        {
            MTIModel mti = _Db.MTIDb.FirstOrDefault(j => j.DocumentNumber == docNo);
            {
                mti.OriginatorName = ishared.GetAccounts().FirstOrDefault(j => j.AccName == docOwner).UserID;
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

            string filePath = Path.Combine(mainDir, docNo);

            ViewData[devType] = Directory.GetFiles(filePath).Where(j => j.Contains(devType)).ToList();

            return Json(null);
        }

        public IActionResult EditDocumentDetails(MTIModel mti)
        {
            MTIModel tempMTI = ishared.GetMTIs().FirstOrDefault(j => j.DocumentNumber == mti.DocumentNumber);

            tempMTI.AssemblyPN = mti.AssemblyPN;
            tempMTI.AssemblyDesc = mti.AssemblyDesc;
            tempMTI.RevNo = mti.RevNo;
            tempMTI.AfterTravLog = mti.AfterTravLog;
            tempMTI.LogsheetDocNo = mti.LogsheetDocNo;
            tempMTI.LogsheetRevNo = mti.LogsheetRevNo;

            if (ModelState.IsValid)
            {
                _Db.MTIDb.Update(tempMTI);
                _Db.SaveChanges();
            }

            return RedirectToAction("MTIList", "Home", new { whichDoc = tempMTI.Product, whichType = tempMTI.DocType});
        }

        public IActionResult EditDocument(string docuno,IFormFile? mpti, IFormFile? bom, IFormFile? schema, IFormFile? drawing, List<IFormFile>? opl,
            List<IFormFile>? derogation, List<IFormFile>? prco, List<IFormFile>? memo, IFormFile? travFile, List<string> DirsTobeDeleted)
        {

            var fromDb = ishared.GetMTIs().FirstOrDefault(j => j.DocumentNumber == docuno);

            MTIModel mod = new MTIModel();
            {
                mod = fromDb;
            }

            if (ModelState.IsValid)
            {
                DocumentNumberVar = docuno;

                DeleteMultipleFiles(DirsTobeDeleted, docuno);
                CopyNoneMultipleDocs(mpti, drawing, bom, schema, travFile);
                CopyMultipleDocs(opl, prco, derogation, memo);

                _Db.MTIDb.Update(mod);
                _Db.SaveChanges();
            }

            return RedirectToAction("MTIView", new {docuNumber = docuno, workStat = false});
        }

        public IActionResult EditDocumentView(string docuNo)
        {
            MTIModel model = ishared.GetMTIs().FirstOrDefault(j => j.DocumentNumber == docuNo)!;

            string filePath = Path.Combine(mainDir, docuNo);

            ViewBag.opl= Directory.GetFiles(filePath).Where(j => j.Contains("OPL")).ToList();
            ViewBag.derogation = Directory.GetFiles(filePath).Where(j => j.Contains("Derogation")).ToList();
            ViewBag.prco = Directory.GetFiles(filePath).Where(j => j.Contains("PRCO")).ToList();
            ViewBag.memo = Directory.GetFiles(filePath).Where(j => j.Contains("EngineeringMemo")).ToList();

            return View(model);
        }

        public IActionResult MTIView(string docuNumber, bool workStat, string sesID)
        {
            MTIModel mti = ishared.GetMTIs().FirstOrDefault(j => j.DocumentNumber == docuNumber);

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

        public ContentResult ValidateDocNo(string DocNo)
        {
            string jsonResponse = "";

            if (ishared.GetMTIs().Any(j => j.DocumentNumber == DocNo)) jsonResponse = "Document Number already exist\r\n";

            return Content(JsonConvert.SerializeObject(new {Failed = jsonResponse}), "application/json");
        }

        private void DeleteMultipleFiles(List<string> fileNames, string docNo)
        {
            foreach (string filename in fileNames)
            {
                System.IO.File.Delete(Path.Combine(mainDir, docNo, $"{filename}.pdf"));
            }
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

        public IActionResult CreateMTIView()
        {
            return View();
        }

        public IActionResult CreateMTI(string documentnumber, string assynumber, string assydesc, string revnumber, 
            IFormFile? assemblydrawing, IFormFile? billsofmaterial, IFormFile? schematicdiagram, IFormFile mpti,
            List<IFormFile>? onepointlesson, List<IFormFile>? prco, List<IFormFile>? derogation, List<IFormFile>? engineeringmemo, 
            string product, string doctype, string originator, IFormFile TravelerFile, string afterTrav, string? docctrlno, string? revnono)
        {

            MTIModel mti = new MTIModel();
            {
                mti.DocumentNumber = documentnumber;
                mti.AssemblyPN = assynumber;
                mti.AssemblyDesc = assydesc;
                mti.RevNo = revnumber;
                mti.Product = product;
                mti.DocType = doctype;
                mti.OriginatorName = ishared.GetAccounts().FirstOrDefault(j => j.AccName == originator).UserID;
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
            }

            return RedirectToAction("MTIList", "Home", new { whichDoc = product });
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

            if (onepointlesson?.Count > 0) files.Add(oplName, onepointlesson);
            if (prco?.Count > 0) files.Add(prcoName, prco);
            if (derogation?.Count > 0) files.Add(derogationName, derogation);
            if (engineeringmemo?.Count > 0) files.Add(memoName, engineeringmemo);

            string filePath = Path.Combine(mainDir, DocumentNumberVar);

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

        public IActionResult ShowDoc(string docunumber, string whichDoc)
        {
            if (whichDoc == "WS")
            {              
                using(FileStream fs = new FileStream(Path.Combine(mainDir, "1_WORKMANSHIP_STANDARD_FOLDER", "WS.pdf"), FileMode.Open))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        fs.CopyTo(ms);
                        return File(ms.ToArray(), "application/pdf");
                    }
                        
                }             
            }
            else if (Directory.GetFiles(Path.Combine(mainDir, docunumber)).Any(j => Path.GetFileName(j) == whichDoc + ".pdf"))
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
        public string AfterTravlog { get; set; }
        public string Product { get; set; }

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
