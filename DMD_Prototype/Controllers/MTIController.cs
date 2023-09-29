using DMD_Prototype.Data;
using Microsoft.AspNetCore.Mvc;
using DMD_Prototype.Models;
using OfficeOpenXml;
using Microsoft.CodeAnalysis;
using NuGet.Packaging.Signing;
using NuGet.Packaging;
using System.Reflection;
using Humanizer;
using System.Net;
using Microsoft.EntityFrameworkCore.Migrations.Internal;

namespace DMD_Prototype.Controllers
{
    public class MTIController : Controller
    {
        private readonly AppDbContext _Db;
        private readonly List<MTIModel> _mtiModel;
        private readonly List<AccountModel> _accounts;

        private readonly string mainDir = "D:\\jtoledo\\Desktop\\DocumentsHere\\";
        private readonly string usersDir = "D:\\jtoledo\\Desktop\\DMD_SessionFolder";
        private readonly string travName = "TravelerFileDoNotEdit.xlsx";

        private readonly string maindoc = "MainDoc";
        private readonly string assydrawing = "AssyDrawing";
        private readonly string bom = "BOM";
        private readonly string schema = "SchematicDiag";
        private readonly string opl = "OPL";
        private readonly string prco = "PRCO";
        private readonly string derogation = "Derogation";
        private readonly string memo = "EngineeringMemo";

        public MTIController(AppDbContext _context)
        {
            _Db = _context;
            _mtiModel = _Db.MTIDb.ToList();
            _accounts = _Db.AccountDb.ToList();
        }

        public IActionResult EditDocument(string docuno, string assyno, string assydesc, string revno,
            IFormFile? mpti, IFormFile? bom, IFormFile? schema, IFormFile? drawing, IFormFile[]? opl,
            IFormFile[]? derogation, IFormFile[]? prco, IFormFile[]? memo, List<string> stepno,
            List<string> task, List<string> division)
        {

            var fromDb = _mtiModel.FirstOrDefault(j => j.DocumentNumber == docuno);

            MTIModel mod = new MTIModel();
            {
                mod = fromDb;
                mod.AssemblyPN = assyno;
                mod.AssemblyDesc = assydesc;
                mod.RevNo = revno;
            }

            if (ModelState.IsValid)
            {
                CreateNewFolderandSaveDocuments(docuno, DictMaker(opl, prco, derogation, memo, null, null, null, null));
                ReplaceDocsInFolder(mpti, drawing, bom, schema, docuno);
                GenerateExcelForTraveler(stepno, task, division, docuno);

                _Db.MTIDb.Update(mod);
                _Db.SaveChanges();
            }

            return RedirectToAction("MTIView", new {docuNumber = docuno, workStat = false});
        }

        public IActionResult EditDocumentView(string docuNo)
        {
            MTIModel model = _mtiModel.FirstOrDefault(j => j.DocumentNumber == docuNo)!;

            MTIViewModel res = new MTIViewModel();
            {
                res.DocumentNumber = docuNo;
                res.AssyNo = model.AssemblyPN;
                res.AssyDesc = model.AssemblyDesc;
                res.RevNo = model.RevNo;
                res.Travelers = TravelerRetriever(docuNo);
            }

            return View(res);
        }

        public IActionResult MTIView(string docuNumber, bool workStat, string sesID, List<string> travelerProgress)
        {
            MTIViewModel mModel = new MTIViewModel();
            {
                mModel.DocumentNumber = docuNumber;
                mModel.Travelers = !workStat? TravelerRetriever(docuNumber) : null;
                mModel.TravProg = travelerProgress;
                mModel.Opl = DeviationDocNames(opl, docuNumber);
                mModel.Prco = DeviationDocNames(prco, docuNumber);
                mModel.Derogation = DeviationDocNames(derogation, docuNumber);
                mModel.Memo = DeviationDocNames(memo, docuNumber);
                mModel.WorkingStat = workStat;
                mModel.SessionID = sesID;
                mModel.AssyNo = _mtiModel.FirstOrDefault(j => j.DocumentNumber == docuNumber).AssemblyPN;
                mModel.AssyDesc = _mtiModel.FirstOrDefault(j => j.DocumentNumber == docuNumber).AssemblyDesc;
                mModel.RevNo = _mtiModel.FirstOrDefault(j => j.DocumentNumber == docuNumber).RevNo;
            }

            return View(mModel);
        }

        private List<string>? DeviationDocNames(string DocName, string docNo)
        {
            List<string>? listOfDocs = new List<string>();
            string folderPath = Path.Combine(mainDir, docNo);

            foreach (string docs in Directory.GetFiles(folderPath))
            {
                string FileNameOnly = Path.GetFileNameWithoutExtension(docs); ;                
                if (docs.Contains(DocName)) listOfDocs.Add(FileNameOnly);
            }

            return listOfDocs;
        }

        private List<TravelerModel> TravelerRetriever(string docuNo)
        {
            int rowCount = 1;
            List<TravelerModel> travelers = new List<TravelerModel>();

            using (ExcelPackage package = new ExcelPackage(new FileInfo(Path.Combine(mainDir, docuNo, travName))))
            {
                if (package.Workbook.Worksheets.Count > 0)
                {
                    do
                    {
                        if (package.Workbook.Worksheets[0].Cells[rowCount, 1]?.Value == null)
                        {
                            break;
                        }

                        TravelerModel trav = new TravelerModel();
                        {
                            trav.StepNumber = package.Workbook.Worksheets[0].Cells[rowCount, 1].Value?.ToString() ?? "";
                            trav.Instruction = package.Workbook.Worksheets[0].Cells[rowCount, 2].Value?.ToString() ?? "";

                            string bythree = package.Workbook.Worksheets[0].Cells[rowCount, 3].Value.ToString();
                            bythree = bythree.Replace("\r", "").Replace("\n", "").Replace("\t", "").Trim();

                            trav.ByThree = bythree;
                        }

                        travelers.Add(trav);

                        rowCount++;
                    } while (true);
                }
            }

            return travelers;
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

        IFormFile ExtensionChecker(IFormFile? file) // neex fixing, still accepts non pdf files
        {
            if (file != null)
            {
                string extension = Path.GetExtension(file.FileName) ?? "";

                if (extension != ".pdf")
                {
                    file = null;
                }
            }

            return file;
        }

        IFormFile[] GroupedExtensionChecker(IFormFile[]? files) // neex fixing, still accepts non pdf files
        {
            List<IFormFile> formFiles = files.ToList();

            foreach(IFormFile file in files)
            {
                if (file != null)
                {
                    string extension = Path.GetExtension(file.FileName) ?? "";

                    if (extension == ".pdf")
                    {
                        formFiles.Add(file);
                    }
                }

            }

            return formFiles.ToArray();
        }

        Dictionary<string, IFormFile[]> DictMaker(IFormFile[]? onepointlesson, IFormFile[]? pRco, IFormFile[]? deRogation, IFormFile[]? engineeringmemo,
            IFormFile? assemblydrawing, IFormFile? billsofmaterial, IFormFile? schematicdiagram, IFormFile? mpti)
        {
            Dictionary<string, IFormFile[]?> files = new Dictionary<string, IFormFile[]?>
                {
                    { maindoc, new[] { mpti } },
                    { assydrawing, new[] { assemblydrawing } },
                    { bom, new[] { billsofmaterial } },
                    { schema, new[] { schematicdiagram } },
                    { opl, onepointlesson },
                    { prco, pRco},
                    { derogation, deRogation },
                    { memo, engineeringmemo }
                };

            return files;
        }

        public IActionResult CreateMTI(string documentnumber, string assynumber, string assydesc, string revnumber, 
            IFormFile assemblydrawing, IFormFile billsofmaterial, IFormFile schematicdiagram, IFormFile mpti,
            IFormFile[]? onepointlesson, IFormFile[]? prco, IFormFile[]? derogation, IFormFile[]? engineeringmemo, 
            List<string> stepNumber, List<string> instruction, List<string> byThree, string product, string doctype,
            string originator)
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
            }

            if (ModelState.IsValid)
            {
                CreateNewFolderandSaveDocuments(documentnumber, DictMaker(onepointlesson, prco, derogation, engineeringmemo, assemblydrawing, billsofmaterial, schematicdiagram, mpti));
                GenerateExcelForTraveler(stepNumber, instruction, byThree, documentnumber);
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

        private void GenerateExcelForTraveler(List<string> stepNumber, List<string> ins, List<string> byThree, string docNo)
        {
            //string folderPath = Path.Combine(mainDir, docNo);
            string filePath = Path.Combine(mainDir, docNo, travName);

            ExcelPackage package = new ExcelPackage();
            {
                var worksheet = package.Workbook.Worksheets.Add(travName);

                int row = 1;
                for (int i = 0; i < stepNumber.Count; i++)
                {
                    worksheet.Cells[row, 1].Value = stepNumber[i];
                    worksheet.Cells[row, 2].Value = ins[i];
                    worksheet.Cells[row, 3].Value = byThree[i];
                    row++;
                }

                package.SaveAs(filePath);
            }

        }

        private void CreateNewFolderandSaveDocuments(string folderName, Dictionary<string, IFormFile[]> files)
        {
            string folderPath = mainDir + folderName;

            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            foreach (var kvp in files)
            {
                foreach (var file in kvp.Value)
                {
                    if (file != null)
                    {
                        string fileName = $"{kvp.Key}.pdf";
                        SaveFileInFolder(file, folderPath, fileName);
                    }
                }
            }
        }

        private void SaveFileInFolder(IFormFile file, string folderPath, string name)
        {
            if (file != null && file.Length > 0)
            {
                string filePath = Path.Combine(folderPath, name);

                if (Directory.Exists(folderPath))
                {
                    int counter = 1;
                    string fileNameOnly = Path.GetFileNameWithoutExtension(name);
                    string extension = Path.GetExtension(name);

                    while (Directory.GetFiles(folderPath).Contains(filePath))
                    {
                        name = $"{fileNameOnly}_{counter}{extension}";
                        filePath = Path.Combine(folderPath, name);
                        counter++;
                    }                   
                }

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }
            }
        }


        private void ReplaceDocsInFolder(IFormFile? mpti, IFormFile? drawing, IFormFile? bOm, IFormFile? sChema, string docuno)
        {
            List<NameSetter> files = new List<NameSetter>();
            if (mpti != null) files.Add(new NameSetter(mpti, maindoc));
            if (drawing != null) files.Add(new NameSetter(drawing, assydrawing));
            if (bOm != null) files.Add(new NameSetter(bOm, bom));
            if (sChema != null) files.Add(new NameSetter(sChema, schema));

            if (files != null && files.Count() > 0)
            {
                foreach (NameSetter file in files)
                {
                    string filePath = Path.Combine(mainDir, docuno, file.Name + ".pdf");
                    using (FileStream fs = new FileStream(filePath, FileMode.Create))
                    {
                        file.Doc.CopyTo(fs);
                    }
                }
            }
        }

        public IActionResult ShowDoc(string docunumber, string whichDoc)
        {
            return File(getDocumentsFromDb(docunumber, whichDoc, ".pdf"), "application/pdf");
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
        public List<TravelerModel>? Travelers { get; set; }
        public List<string>? TravProg { get; set; }
        public bool WorkingStat { get; set; } = false;
        public string? SessionID { get; set; }

    }
}
