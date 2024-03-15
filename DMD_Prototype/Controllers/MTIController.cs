using DMD_Prototype.Data;
using Microsoft.AspNetCore.Mvc;
using DMD_Prototype.Models;
using Microsoft.CodeAnalysis;
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

        public string[] GetUserData()
        {
            string[] userData = TempData["EN"] as string[];

            TempData.Keep();

            return userData;
        }

        public async Task<IActionResult> ChangeDocOwner(string docNo, string  docOwner)
        {
            MTIModel mti = _Db.MTIDb.FirstOrDefault(j => j.DocumentNumber == docNo);
            {
                mti.OriginatorName = (await ishare.GetAccounts()).FirstOrDefault(j => j.AccName == docOwner).UserID;
            }

            if (ModelState.IsValid)
            {
                _Db.MTIDb.Update(mti);
                await _Db.SaveChangesAsync();
            }

            return RedirectToAction("MTIList", "Home", new {whichDoc = mti.Product, whichType = mti.DocType});
        }

        public async Task<IActionResult> DeleteDeviationDoc(string dir, string devType, string docNo)
        {
            System.IO.File.Delete(dir);

            string filePath = Path.Combine(await ishare.GetPath("mainDir"), docNo);

            ViewData[devType] = Directory.GetFiles(filePath).Where(j => j.Contains(devType)).ToList();

            return Json(null);
        }

        public async Task<IActionResult> EditDocumentDetails(MTIModel mti, string user)
        {
            MTIModel tempMTI = (await ishare.GetMTIs()).FirstOrDefault(j => j.DocumentNumber == mti.DocumentNumber && !j.isDeleted);

            tempMTI.AssemblyPN = mti.AssemblyPN;
            tempMTI.AssemblyDesc = mti.AssemblyDesc;
            tempMTI.RevNo = mti.RevNo;
            tempMTI.AfterTravLog = mti.AfterTravLog;
            tempMTI.LogsheetDocNo = mti.LogsheetDocNo;
            tempMTI.LogsheetRevNo = mti.LogsheetRevNo;
            tempMTI.ObsoleteStat = mti.ObsoleteStat;
            tempMTI.DocType = mti.DocType;

            if (ModelState.IsValid)
            {
                string message = $", updated details of engineering Document with Doc Number of {mti.DocumentNumber}.";
                if (mti.ObsoleteStat) message = $", marked {mti.DocumentNumber} as obsolete.";

                await ishare.RecordOriginatorAction(message, user, DateTime.Now);
                _Db.MTIDb.Update(tempMTI);
                await _Db.SaveChangesAsync();
            }

            return RedirectToAction("MTIList", "Home", new { whichDoc = tempMTI.Product, whichType = tempMTI.DocType});
        }

        public async Task<IActionResult> EditDocument(string docuno,IFormFile? mpti, IFormFile? bom, IFormFile? schema, IFormFile? drawing, List<IFormFile>? opl,
            List<IFormFile>? derogation, List<IFormFile>? prco, List<IFormFile>? memo, IFormFile? travFile, List<string> DirsTobeDeleted, char mtpistatus,
            string user, string revNo, bool emailPL, string? remarks)
        {

            var fromDb = (await ishare.GetMTIs()).FirstOrDefault(j => j.DocumentNumber == docuno);
            MTIModel mod = new MTIModel();
            {
                mod = fromDb;
                mod.MTPIStatus = mtpistatus;
                mod.RevNo = revNo;
            }

            if (ModelState.IsValid)
            {
                DocumentNumberVar = docuno;

                if (DirsTobeDeleted.Count > 0) await DeleteMultipleFiles(DirsTobeDeleted, docuno);
                await CopyNoneMultipleDocs(mpti, drawing, bom, schema, travFile);
                CopyMultipleDocs(opl, prco, derogation, memo);

                _Db.MTIDb.Update(mod);

                await _Db.SaveChangesAsync();
            }

            if (emailPL)
            {
                string statusSetter = mtpistatus == 'c' ? "Controlled" : "Interim";
                string remarkSetter = string.IsNullOrEmpty(remarks) ? "No remarks" : remarks;
                string subject = $"DMD Portal, Document update {statusSetter}";
                string body = $"Good day!\r\n{user} has updated a document/s with document number of {docuno}, having the details of the following:\r\n\r\nStatus: {statusSetter}\r\nAssembly Part Number: {mod.AssemblyPN}\r\nDescription: {mod.AssemblyDesc}\r\nRevision Number: {mod.RevNo}\r\nRemaks:{remarkSetter}\r\n\r\nThis is a system generated email, please do not reply. Thank you and have a great day!";
                await ishare.SendEmailNotification((await ishare.GetMultipleusers("PL_INTERVENOR")).ToList(), subject, body);
            }

            return RedirectToAction("MTIView", new {docuNumber = docuno, workStat = false});
        }

        public async Task<IActionResult> EditDocumentView(string docuNo)
        {
            MTIModel model = (await ishare.GetMTIs()).FirstOrDefault(j => j.DocumentNumber == docuNo)!;

            string filePath = Path.Combine(await ishare.GetPath("mainDir"), docuNo);

            ViewBag.opl= Directory.GetFiles(filePath).Where(j => j.Contains("(o)")).ToList();
            ViewBag.derogation = Directory.GetFiles(filePath).Where(j => j.Contains("(d)")).ToList();
            ViewBag.prco = Directory.GetFiles(filePath).Where(j => j.Contains("(p)")).ToList();
            ViewBag.memo = Directory.GetFiles(filePath).Where(j => j.Contains("(m)")).ToList();

            return View(model);
        }

        public async Task<IActionResult> MTIView(string docuNumber, bool workStat, string sesID)
        {
            if (!System.IO.File.Exists(Path.Combine(await ishare.GetMainDocsPath(), docuNumber, await ishare.GetMainDocName())))
            {
                return NotFound("The Document you are working on or looking for could be deleted, not found or missing. Please contact the document's originator.");
            }

            MTIModel mti = (await ishare.GetMTIs()).FirstOrDefault(j => j.DocumentNumber == docuNumber && !j.isDeleted);

            MTIViewModel mModel = new();
            mModel.DocumentNumber = docuNumber;
            mModel.Opl = await DeviationDocNames("o", docuNumber);
            mModel.Prco = await DeviationDocNames("p", docuNumber);
            mModel.Derogation = await DeviationDocNames("d", docuNumber);
            mModel.Memo = await DeviationDocNames("m", docuNumber);
            mModel.WorkingStat = workStat;
            mModel.SessionID = sesID;
            mModel.AssyNo = mti.AssemblyPN;
            mModel.AssyDesc = mti.AssemblyDesc;
            mModel.RevNo = mti.RevNo;
            mModel.AfterTravlog = mti.AfterTravLog;
            mModel.Product = mti.Product;
            mModel.DocType = mti.DocType;
            mModel.ObsoleteStat = mti.ObsoleteStat;
            mModel.AssyDrawing = System.IO.File.Exists(Path.Combine(await ishare.GetPath("mainDir"), docuNumber, await ishare.GetAssyDrawingName()));
            mModel.Bom = System.IO.File.Exists(Path.Combine(await ishare.GetPath("mainDir"), docuNumber, await ishare.GetBOMName()));
            mModel.SchematicDiagram = System.IO.File.Exists(Path.Combine(await ishare.GetPath("mainDir"), docuNumber, await ishare.GetSchemaDiagramName()));

            return View(mModel);
        }

        public async Task<ContentResult> ValidateDocNo(string DocNo)
        {
            string jsonResponse = "";

            if ((await ishare.GetMTIs()).Any(j => j.DocumentNumber == DocNo)) jsonResponse = "Document Number already exist\r\n";

            return Content(JsonConvert.SerializeObject(new {Failed = jsonResponse}), "application/json");
        }

        private async Task DeleteMultipleFiles(List<string> fileNames, string docNo)
        {
            IEnumerable<string> files = Directory.GetFiles(Path.Combine(await ishare.GetPath("mainDir"), docNo));

            foreach (string filename in fileNames)
            {
                string filePath = Path.Combine(await ishare.GetPath("mainDir"), docNo, filename);

                if (!System.IO.File.Exists(filePath))
                {
                    filePath = files.FirstOrDefault(j => j == filename);
                }

                System.IO.File.Delete(filePath);

                ishare.RecordOriginatorAction($"Deleted {Path.GetFileName(filename).Substring(3)} from {docNo}.", GetUserData()[0], DateTime.Now);
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

        private async Task<List<string>?> DeviationDocNames(string DocName, string docNo)
        {
            List<string>? listOfDocs = new List<string>();
            string folderPath = Path.Combine(await ishare.GetPath("mainDir"), docNo);

            if (Directory.GetFiles(folderPath).Count() > 0)
            {
                foreach (string docs in Directory.GetFiles(folderPath))
                {
                    string fileName = Path.GetFileName(docs);

                    if (docs.Contains(Path.GetFileNameWithoutExtension($"({DocName})"))) listOfDocs.Add(fileName);
                }
            }

            return listOfDocs;
        }

        public IActionResult CreateMTIView()
        {
            return View();
        }

        public async Task<IActionResult> CreateMTI(string documentnumber, string assynumber, string assydesc, string revnumber, 
            IFormFile? assemblydrawing, IFormFile? billsofmaterial, IFormFile? schematicdiagram, IFormFile mpti,
            List<IFormFile>? onepointlesson, List<IFormFile>? prco, List<IFormFile>? derogation, List<IFormFile>? engineeringmemo, 
            string product, string doctype, string originator, IFormFile TravelerFile, string afterTrav, string? logsheetDocNo, string? logsheetRevNo)
        {

            MTIModel mti = new MTIModel();
            mti.DocumentNumber = documentnumber.ToUpper();
            mti.AssemblyPN = assynumber;
            mti.AssemblyDesc = assydesc;
            mti.RevNo = revnumber;
            mti.Product = product;
            mti.DocType = doctype;
            mti.OriginatorName = (await ishare.GetAccounts()).FirstOrDefault(j => j.AccName == originator).UserID;
            mti.AfterTravLog = afterTrav;
            mti.LogsheetDocNo = logsheetDocNo;
            mti.LogsheetRevNo = logsheetRevNo;

            if (ModelState.IsValid)
            {
                DocumentNumberVar = documentnumber;

                await CreateNewFolder(documentnumber);
                await CopyNoneMultipleDocs(mpti, assemblydrawing, billsofmaterial, schematicdiagram, TravelerFile);
                CopyMultipleDocs(onepointlesson, prco, derogation, engineeringmemo);
                await ishare.RecordOriginatorAction($"Uploaded engineering documents/s with Doc Number of {documentnumber}.", originator, DateTime.Now);

                _Db.MTIDb.Add(mti);
                await _Db.SaveChangesAsync();
            }

            return RedirectToAction("MTIList", "Home", new { whichDoc = product, whichType = mti.DocType});
        }

        private async Task CreateNewFolder(string docNumber)
        {
            Directory.CreateDirectory(Path.Combine(await ishare.GetPath("mainDir"), docNumber));
        }

        private async Task CopyNoneMultipleDocs(IFormFile? mainDoc, IFormFile? AssyDrawing, IFormFile? BOM, IFormFile? Schematic, IFormFile? Traveler)
        {
            Dictionary<string, IFormFile> files = new Dictionary<string, IFormFile>();

            if (mainDoc != null) files.Add(await ishare.GetPath("mainDoc"), mainDoc);
            if (AssyDrawing != null) files.Add(await ishare.GetPath("assy"), AssyDrawing);
            if (BOM  != null) files.Add(await ishare.GetPath("bom"), BOM);
            if (Schematic != null) files.Add(await ishare.GetPath("schema"), Schematic);
            if (Traveler != null) files.Add(await ishare.GetPath("travName"), Traveler);

            foreach (var file in files)
            {
                using (FileStream fs = new FileStream(Path.Combine(await ishare.GetPath("mainDir"), DocumentNumberVar, file.Key), FileMode.Create))
                {
                    file.Value.CopyTo(fs);
                    await ishare.RecordOriginatorAction($"Uploaded {Path.GetFileName(fs.Name)} to {DocumentNumberVar}.", GetUserData()[0], DateTime.Now);
                }
            }
        }

        private async Task CopyMultipleDocHandler(List<IFormFile>? files, char whichDoc)
        {
            string filePath = Path.Combine(await ishare.GetPath("mainDir"), DocumentNumberVar);

            foreach (IFormFile file in files)
            {
                using (FileStream fs = new FileStream(Path.Combine(filePath, $"({whichDoc}){file.FileName}"), FileMode.Create))
                {
                    file.CopyTo(fs);
                    await ishare.RecordOriginatorAction($"Uploaded {Path.GetFileName(fs.Name).Substring(3)} to {DocumentNumberVar}.", GetUserData()[0], DateTime.Now);
                }
            }
        }

        private void CopyMultipleDocs(List<IFormFile>? onepointlesson, List<IFormFile>? prco, List<IFormFile>? derogation, List<IFormFile>? engineeringmemo)
        {
            if (onepointlesson != null && onepointlesson.Count > 0) CopyMultipleDocHandler(onepointlesson, 'o');
            if (prco != null && prco.Count > 0) CopyMultipleDocHandler(prco, 'p');
            if (derogation != null && derogation.Count > 0) CopyMultipleDocHandler(derogation, 'd');
            if (engineeringmemo != null && engineeringmemo.Count > 0) CopyMultipleDocHandler(engineeringmemo, 'm');
        }

        public async Task<IActionResult> UploadWS(IFormFile file)
        {
            using (FileStream fs = new FileStream(Path.Combine(await ishare.GetPath("mainDir"), await ishare.GetPath("wsf"), await ishare.GetPath("ws")), FileMode.Create))
            {
                file.CopyTo(fs);
            }

            return RedirectToAction("Index", "Home");
        }

        private async Task<byte[]?> getDocumentsFromDb(string docuNumber, string whichDoc, char? docType)
        {
            string folderPath = "";
            IEnumerable<string> files;

            if (docType == null)
            {
                folderPath = Path.Combine(await ishare.GetPath("mainDir"), docuNumber, whichDoc);
            }
            else
            {
                folderPath = Path.Combine(await ishare.GetPath("mainDir"), docuNumber, $"({docType}){whichDoc}");
            }

            if (System.IO.File.Exists(folderPath))
            {
                files = Directory.GetFiles(Path.Combine(await ishare.GetPath("mainDir"), docuNumber));
                folderPath = files.FirstOrDefault(j => j.Contains(whichDoc));

                using (FileStream fileStream = new FileStream(folderPath, FileMode.Open))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        fileStream.CopyTo(ms);
                        return ms.ToArray();
                    }
                }
            }
            else
            {
                return null;
            }
        }

        public async Task<IActionResult> ShowDoc(string docunumber, string whichDoc, char? docType)
        {

            byte[]? file = await getDocumentsFromDb(docunumber, whichDoc, docType);

            if (file == null)
            {
                return NoContent();
            }
            else
            {
                return File(file, "application/pdf");
            }
        }

        public async Task<IActionResult> ShowWS()
        {
            if (System.IO.File.Exists(Path.Combine(await ishare.GetPath("mainDir"), await ishare.GetPath("wsf"), await ishare.GetPath("ws"))))
            {
                using (FileStream fs = new FileStream(Path.Combine(await ishare.GetPath("mainDir"), await ishare.GetPath("wsf"), await ishare.GetPath("ws")), FileMode.Open))
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
        public bool AssyDrawing { get; set; } = false;
        public bool Bom { get; set; } = false;
        public bool SchematicDiagram { get; set; } = false;

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
