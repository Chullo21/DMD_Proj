using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NuGet.Protocol;
using OfficeOpenXml;
using OfficeOpenXml.Sorting;
using System.IO;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace DMD_Prototype.Controllers
{
    public class DocumentController : Controller
    {
        private readonly ISharedFunct ishared;

        public DocumentController(ISharedFunct ishared)
        {
            this.ishared = ishared;
        }

        public async Task<ContentResult> GetLogsheetDataForEdit(string sessionId, string logType)
        {
            if (logType.ToLower() == "c")
            {
                return await GetAllLogsheet(sessionId, await new WorkController(null, this.ishared).GetConfigTemplate(sessionId));
            }
            else
            {
                return await GetAllLogsheet(sessionId, await new WorkController(null, this.ishared).GetTELTemplate(sessionId));
            }
        }

        public async Task<ContentResult> GetAllLogsheet(string sessionId, ConfigDataTemplate config)
        {
            List<ConfigDataForEdit> res = new List<ConfigDataForEdit>();

            using (ExcelPackage package = new ExcelPackage(Path.Combine(await ishared.GetPath("userDir"), sessionId, await ishared.GetPath("logName"))))
            {
                int page = 0;
                
                int totalPages = package.Workbook.Worksheets.Count - 1;
                var ws = package.Workbook.Worksheets[$"P1"];

                int row = int.Parse(Regex.Replace(config.StartPartNumber, "[^0-9]", ""));
                string partLocator = Regex.Replace(config.StartPartNumber, "[^a-zA-Z]", "");
                string descriptionLocator = Regex.Replace(config.StartDescription, "[^a-zA-Z]", "");
                string serialLocator = Regex.Replace(config.StartSerialNumber, "[^a-zA-Z]", "");

                do
                {
                    if (row > int.Parse(Regex.Replace(config.LastRow, "[^0-9]", "")))
                    {
                        page++;
                        ws = package.Workbook.Worksheets[$"P{page + 1}"];
                        row = 10;
                    }

                    if (page + 1 > totalPages || (ws.Cells[$"{partLocator}{row}"].Value == null && ws.Cells[$"{descriptionLocator}{row}"].Value == null && ws.Cells[$"{serialLocator}{row}"].Value == null))
                    {
                        break;
                    }

                    ConfigDataForEdit data = new();

                    data.PN = ws.Cells[row, 1].Value == null ? "" : ws.Cells[$"{partLocator}{row}"].Value.ToString();
                    data.Desc = ws.Cells[row, 3].Value == null ? "" : ws.Cells[$"{descriptionLocator}{row}"].Value.ToString();
                    data.Parameter = ws.Cells[row, 7].Value == null ? "" : ws.Cells[$"{serialLocator}{row}"].Value.ToString();

                    res.Add(data);

                    row += int.Parse(Regex.Replace(config.IncrementValue, "[^0-9]", ""));

                } while (true);
            }

            string jsonContent = JsonConvert.SerializeObject(new { r = res });
            return Content(jsonContent, "application/json");
        }

        public async Task<ContentResult> GetAllLogsheet(string sessionId, TELDataTemplate config)
        {
            List<ConfigDataForEdit> res = new List<ConfigDataForEdit>();

            using (ExcelPackage package = new ExcelPackage(Path.Combine(await ishared.GetPath("userDir"), sessionId, await ishared.GetPath("logName"))))
            {
                int page = 0;

                int totalPages = package.Workbook.Worksheets.Count - 1;
                var ws = package.Workbook.Worksheets[$"P1"];

                int row = int.Parse(Regex.Replace(config.StartFAPNumber, "[^0-9]", ""));
                string partLocator = Regex.Replace(config.StartFAPNumber, "[^a-zA-Z]", "");
                string descriptionLocator = Regex.Replace(config.StartDescription, "[^a-zA-Z]", "");
                string serialLocator = Regex.Replace(config.StartCalibrationDueDate, "[^a-zA-Z]", "");

                do
                {
                    if (row > int.Parse(Regex.Replace(config.LastRow, "[^0-9]", "")))
                    {
                        page++;
                        ws = package.Workbook.Worksheets[$"P{page + 1}"];
                        row = 10;
                    }

                    if (page + 1 > totalPages || (ws.Cells[$"{partLocator}{row}"].Value == null && ws.Cells[$"{descriptionLocator}{row}"].Value == null && ws.Cells[$"{serialLocator}{row}"].Value == null))
                    {
                        break;
                    }

                    ConfigDataForEdit data = new();

                    data.PN = ws.Cells[row, 1].Value == null ? "" : ws.Cells[$"{partLocator}{row}"].Value.ToString();
                    data.Desc = ws.Cells[row, 3].Value == null ? "" : ws.Cells[$"{descriptionLocator}{row}"].Value.ToString();
                    data.Parameter = ws.Cells[row, 7].Value == null ? "" : ws.Cells[$"{serialLocator}{row}"].Value.ToString();

                    res.Add(data);

                    row += int.Parse(Regex.Replace(config.IncrementValue, "[^0-9]", ""));

                } while (true);
            }

            string jsonContent = JsonConvert.SerializeObject(new { r = res });
            return Content(jsonContent, "application/json");
        }



        public async Task<ContentResult> SaveTravChanges(string[] Step, string[] Instruction, string[] SinglePara, string[] FirstThreePara, string[] SecondThreePara, string[] ThirdThreePara, bool[] isMerge, string sessionId)
        {
            string filePath = Path.Combine(await ishared.GetPath("userDir"), sessionId, await ishared.GetPath("userTravName"));

            using(ExcelPackage package = new(filePath))
            {
                var ws = package.Workbook.Worksheets[0];

                int row = 11;

                for(int i = 0; i < Step.Length; i++)
                {
                    if (isMerge[i])
                    {
                        ws.Cells[row, 9].Value = SinglePara[i];
                    }
                    else
                    {                       
                        ws.Cells[row, 9].Value = FirstThreePara[i];
                        ws.Cells[row, 10].Value = SecondThreePara[i];
                        ws.Cells[row, 11].Value = ThirdThreePara[i];
                    }

                    row++;
                }

                package.Save();
            }

            return Content("", "application/json");
        }

        public async Task<ContentResult> SaveConfigChanges(string[] PN, string[] Desc, string[] Parameter, string sessionId)
        {
            string filePath = Path.Combine(await ishared.GetPath("userDir"), sessionId, await ishared.GetPath("logName"));

            using (ExcelPackage package = new(filePath))
            {
                int page = 0;
                int indexCounter = 0;
                var ws = package.Workbook.Worksheets[page];

                int row = 10;

                foreach(string entry in PN)
                {
                    if (row >= 49)
                    {
                        row = 10;
                        page++;
                        ws = package.Workbook.Worksheets[page];
                    }

                    ws.Cells[row, 1].Value = PN[indexCounter];
                    ws.Cells[row, 3].Value = Desc[indexCounter];
                    ws.Cells[row, 7].Value = Parameter[indexCounter];

                    indexCounter++;
                    row += 3;
                }

                package.Save();
            }

            return Content("", "application/json");
        }

        public async Task<IActionResult> GetTravelerSampleFromDisposable(string path)
        {
            byte[] result;

            using (FileStream fileStream = new FileStream(path, FileMode.Open))
            {
                using(MemoryStream ms = new())
                {
                    await fileStream.CopyToAsync(ms);
                    result = ms.ToArray();
                }
                
            }

            System.IO.File.Delete(path);
            return File(result, "application/pdf");
        }

        //public async Task<IActionResult> UpdateDocumentTemplates(IFormFile? config, IFormFile? test, IFormFile? mpi, IFormFile? mti)
        //{
        //    try
        //    {
        //        if (config != null) await UpdateDocument(config, await ishared.GetPath("configDir"));

        //        if (test != null) await UpdateDocument(test, await ishared.GetPath("testDir"));

        //        if (mpi != null) await UpdateDocument(config, await ishared.GetPath("mpiDir"));

        //        if (mti != null) await UpdateDocument(config, await ishared.GetPath("mtiDir"));

        //        return Json(new { message = "success" });
        //    }
        //    catch(Exception ex)
        //    {
        //        return Json(new { message = ex.Message });
        //    }
        //}

        //private async Task UpdateDocument(IFormFile file, string locationDir)
        //{
        //    using (FileStream fs = new FileStream(Path.Combine(locationDir), FileMode.Create))
        //    {
        //        file.CopyTo(fs);
        //    }
        //}

        public async Task<IActionResult> UpdateTemplate(IFormFile file, string model, string directory)
        {
            try
            {
                using (ExcelPackage package = new(file.OpenReadStream()))
                {
                    package.Workbook.Worksheets.Add("Configuration");
                    //package.Workbook.Worksheets["Configuration"].Hidden = eWorkSheetHidden.VeryHidden;

                    var ws = package.Workbook.Worksheets["Configuration"];

                    package.Workbook.Worksheets[0].Name = "P1";

                    ws.Cells["A1"].Value = model;

                    package.SaveAs(await ishared.GetPath(directory));
                }
            }
            catch(Exception ex)
            {
                return Json(new { message = ex.Message });
            }

            return Json(new { message = "success" });
        }

        public async Task<IActionResult> UpdateTravelerTemplate(string config, string directory, IFormFile file, string travelerType)
        {
            try
            {
                System.IO.File.WriteAllText(await ishared.GetPath(directory), config);
                //System.IO.File.Copy(file.OpenReadStream(), await ishared.GetPath(directory), true);
                using(FileStream fs = new(await ishared.GetPath(travelerType), FileMode.Create))
                {
                    file.CopyTo(fs);
                }
            }
            catch(Exception ex)
            {
                return Json(new {message = ex.Message});
            }

            return Json(new { message = "success" });
        }

        //public async Task<IActionResult> UpdateTravelerTemplate(IFormFile file, string config, string directory)
        //{
        //    try
        //    {
        //        System.IO.File.WriteAllText(await ishared.GetPath(directory), config);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { message = ex.Message });
        //    }

        //    return Json(new { message = "success" });
        //}

        public async Task<IActionResult> TryTravelerTemplate(string directory, IFormFile template, char travelerType)
        {
            string config = System.IO.File.ReadAllText(await ishared.GetPath(directory));
            string path = "";
            dynamic temp;

            using (ExcelPackage package = new(template.OpenReadStream()))
            {
                if (travelerType == 'p')
                {
                    temp = JsonConvert.DeserializeObject<TravelerMPITemplate>(config);
                }
                else
                {
                    temp = JsonConvert.DeserializeObject<TravelerMTITemplate>(config);
                }

                path = new DMDLibrary.COMHandler().ConvertExcelIntoPDFThenByte(PerformTravelerTrial(package, temp), await ishared.GetPath("disDir"));

                return Json(new { path = path });
            }            
        }

        public async Task<IActionResult> TryLogsheetTemplate(string directory, char logsheetType)
        {
            MemoryStream file = new();
            string path = "";
            dynamic temp;

            using(ExcelPackage package = new(await ishared.GetPath(directory)))
            {
                string jsonString = package.Workbook.Worksheets["Configuration"].Cells["A1"].Value.ToString();

                if (logsheetType == 'c')
                {
                    temp = JsonConvert.DeserializeObject<ConfigDataTemplate>(jsonString);
                }
                else
                {
                    temp = JsonConvert.DeserializeObject<TELDataTemplate>(jsonString);
                }

                path = new DMDLibrary.COMHandler().ConvertExcelIntoPDFThenByte(await PerformLogsheetTrial(package, temp), await ishared.GetPath("disDir"));

            }

            using (FileStream fileStream = new FileStream(path, FileMode.Open))
            {
                await fileStream.CopyToAsync(file);
            }

            System.IO.File.Delete(path);

            return File(file.ToArray(), "application/pdf");
        }

        private ExcelPackage PerformTravelerTrial(ExcelPackage package, TravelerMPITemplate config)
        {
            try
            {
                var ws = package.Workbook.Worksheets[0];

                ws.Cells[config.AssemblyPartNumber].Value = "Assembly Part Number";
                ws.Cells[config.Description].Value = "Description Example";
                ws.Cells[config.DocumentControlNumber].Value = "Document Control Number";
                ws.Cells[config.ReferenceMPINumber].Value = "Reference";
                ws.Cells[config.RevisionNumber].Value = "Revision Number";
                ws.Cells[config.WorkOrder].Value = "Work Order";
                ws.Cells[config.SerialNumber].Value = "Serial Number";
                ws.Cells[config.StartDate].Value = "Start Date";
                ws.Cells[config.CompleteDate].Value = "Complete Date";

                if (config.Page != null) ws.Cells[config.Page].Value = "Page";

                int startingRow = int.Parse(Regex.Replace(config.StartStepNumber, "[^0-9]", ""));

                for (int i = startingRow; i <= int.Parse(config.LastRow); i += int.Parse(config.IncrementValue))
                {
                    ws.Cells[$"{Regex.Replace(config.StartStepNumber, "[0-9]", "")}{i}"].Value = "Step Number Example";
                    ws.Cells[$"{Regex.Replace(config.StartTask, "[0-9]", "")}{i}"].Value = "Task Example";
                    ws.Cells[$"{Regex.Replace(config.StartTechnician, "[0-9]", "")}{i}"].Value = "Calibration Due Date Example";
                    ws.Cells[$"{Regex.Replace(config.StartDateParameter, "[0-9]", "")}{i}"].Value = "Date Example";
                }               
            }
            catch(Exception ex)
            {
                return package;
            }

            return package;
        }

        private ExcelPackage PerformTravelerTrial(ExcelPackage package, TravelerMTITemplate config)
        {
            var ws = package.Workbook.Worksheets[0];
            try
            {
                ws.Cells[config.AssemblyPartNumber].Value = "Assembly Part Number";
                ws.Cells[config.Description].Value = "Description Example";
                ws.Cells[config.DocumentControlNumber].Value = "Document Control Number";
                ws.Cells[config.Reference].Value = "Reference";
                ws.Cells[config.RevisionNumber].Value = "Revision Number";
                ws.Cells[config.WorkOrder].Value = "Work Order";
                ws.Cells[config.SerialNumber].Value = "Serial Number";
                ws.Cells[config.StartDate].Value = "Start Date";
                ws.Cells[config.CompleteDate].Value = "Complete Date";

                if (!string.IsNullOrEmpty(config.Page)) ws.Cells[config.Page].Value = "Page";
                
            }
            catch (Exception ex)
            {
                return package;
            }

            try
            {
                int startingRow = int.Parse(Regex.Replace(config.StartStepNumber, "[^0-9]", ""));

                for (int i = startingRow; i <= int.Parse(config.LastRow); i += int.Parse(config.IncrementValue))
                {
                    ws.Cells[$"{Regex.Replace(config.StartStepNumber, "[0-9]", "")}{i}"].Value = "Step Number Example";
                    ws.Cells[$"{Regex.Replace(config.StartTask, "[0-9]", "")}{i}"].Value = "Task Example";
                    ws.Cells[$"{Regex.Replace(config.StartTechnicianAndDate, "[0-9]", "")}{i}"].Value = "Calibration Due Date Example";

                    if (!ws.Cells[$"{Regex.Replace(config.StartParameter, "[0-9]", "")}{i}"].Merge)
                    {
                        ws.Cells[$"{Regex.Replace(config.StartParameter, "[0-9]", "")}{i}"].Value = "Date Example";
                    }
                    else
                    {
                        char byThreeChar = char.Parse(Regex.Replace(config.StartParameter, "[^a-zA-Z]", ""));
                        int byThreeToASCII = (int)byThreeChar;
                        char byThreeFirst = (char)(byThreeToASCII);
                        char firstByThree = (char)(byThreeToASCII + 1);
                        char secondByThree = (char)(byThreeToASCII + 2);

                        ws.Cells[$"{byThreeFirst}{i}"].Value = "Date Example";
                        ws.Cells[$"{firstByThree}{i}"].Value = "Date Example";
                        ws.Cells[$"{secondByThree}{i}"].Value = "Date Example";
                    }
                }
            }
            catch (Exception ex)
            {
                return package;
            }

            return package;
        }

        public async Task<ExcelPackage> PerformLogsheetTrial(ExcelPackage package, ConfigDataTemplate config)
        {
            var ws = package.Workbook.Worksheets[0];

            ws.Cells[config.AssemblyPartNumber].Value = "Assembly Part Number";
            ws.Cells[config.AssemblyDescription].Value = "Assembly Description";
            ws.Cells[config.DocumentControlNumber].Value = "Document Control Number";
            ws.Cells[config.Reference].Value = "Reference";
            ws.Cells[config.RevisionNumber].Value = "Revision Number";
            ws.Cells[config.WorkOrder].Value = "Work Order";
            ws.Cells[config.SerialNumber].Value = "Serial Number";
            ws.Cells[config.StartDate].Value = "Start Date";
            ws.Cells[config.CompleteDate].Value = "Complete Date";
            ws.Cells[config.Page].Value = "Page";

            int startingRow = int.Parse(Regex.Replace(config.StartPartNumber, "[^0-9]", ""));

            for (int i = startingRow; i <= int.Parse(config.LastRow); i += int.Parse(config.IncrementValue))
            {
                ws.Cells[$"{Regex.Replace(config.StartPartNumber, "[0-9]", "")}{i}"].Value = "Page Number Example";
                ws.Cells[$"{Regex.Replace(config.StartDescription, "[0-9]", "")}{i}"].Value = "Description Number Example";
                ws.Cells[$"{Regex.Replace(config.StartSerialNumber, "[0-9]", "")}{i}"].Value = "Calibration Due Date Example";
            }

            package.Workbook.Worksheets.Delete(package.Workbook.Worksheets[1]);

            return package;
        }

        public async Task<ExcelPackage> PerformLogsheetTrial(ExcelPackage package, TELDataTemplate config)
        {
            var ws = package.Workbook.Worksheets[0];

            ws.Cells[config.AssemblyPartNumber].Value = "Assembly Part Number";
            ws.Cells[config.AssemblyDescription].Value = "Assembly Description";
            ws.Cells[config.DocumentControlNumber].Value = "Document Control Number";
            ws.Cells[config.Reference].Value = "Reference";
            ws.Cells[config.RevisionNumber].Value = "Revision Number";
            ws.Cells[config.WorkOrder].Value = "Work Order";
            ws.Cells[config.SerialNumber].Value = "Serial Number";
            ws.Cells[config.StartDate].Value = "Start Date";
            ws.Cells[config.CompleteDate].Value = "Complete Date";
            ws.Cells[config.Page].Value = "Page";

            int startingRow = int.Parse(Regex.Replace(config.StartFAPNumber, "[^0-9]", ""));

            for (int i = startingRow; i <= int.Parse(config.LastRow); i += int.Parse(config.IncrementValue))
            {
                ws.Cells[$"{Regex.Replace(config.StartFAPNumber, "[0-9]", "")}{i}"].Value = "Page Number Example";
                ws.Cells[$"{Regex.Replace(config.StartDescription, "[0-9]", "")}{i}"].Value = "Description Number Example";
                ws.Cells[$"{Regex.Replace(config.StartCalibrationDueDate, "[0-9]", "")}{i}"].Value = "Calibration Due Date Example";
            }

            package.Workbook.Worksheets.Delete(package.Workbook.Worksheets[1]);

            return package;
        }

        //public async Task<IActionResult> UpdateTemplate(IFormFile file, TELDataTemplate model)
        //{
        //    try
        //    {
        //        using (ExcelPackage package = new(file.OpenReadStream()))
        //        {
        //            package.Workbook.Worksheets.Add("Configuration");

        //            var ws = package.Workbook.Worksheets[1];

        //            ws.Cells["A1"].Value = model.ToJson();

        //            package.SaveAs(await ishared.GetPath("testDir"));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { message = ex.Message });
        //    }

        //    return Json(new { });
        //}
    }

    public class ConfigDataForEdit
    {
        public string PN { get; set; }
        public string Desc { get; set; }
        public string Parameter { get; set; }
    }

    public class ConfigDataTemplate
    {
        public string AssemblyPartNumber { get; set; }
        public string  AssemblyDescription { get; set; }
        public string DocumentControlNumber { get; set; }
        public string Reference { get; set; }
        public string RevisionNumber { get; set; }
        public string WorkOrder { get; set; }
        public string SerialNumber { get; set; }
        public string StartDate { get; set; }
        public string CompleteDate { get; set; }
        public string Page { get; set; }
        public string StartPartNumber { get; set; }
        public string StartDescription { get; set; }
        public string StartSerialNumber { get; set; }
        public string IncrementValue { get; set; }
        public string LastRow { get; set; }
    }

    public class TELDataTemplate
    {
        public string AssemblyPartNumber { get; set; }
        public string AssemblyDescription { get; set; }
        public string DocumentControlNumber { get; set; }
        public string Reference { get; set; }
        public string RevisionNumber { get; set; }
        public string WorkOrder { get; set; }
        public string SerialNumber { get; set; }
        public string StartDate { get; set; }
        public string CompleteDate { get; set; }
        public string Page { get; set; }
        public string StartFAPNumber { get; set; }
        public string StartDescription { get; set; }
        public string StartCalibrationDueDate { get; set; }
        public string IncrementValue { get; set; }
        public string LastRow { get; set; }
    }

    public class TravelerMPITemplate
    {
        public string AssemblyPartNumber { get; set; }
        public string Description { get; set; }
        public string DocumentControlNumber { get; set; }
        public string ReferenceMPINumber { get; set; }
        public string RevisionNumber { get; set; }
        public string WorkOrder { get; set; }
        public string SerialNumber { get; set; }
        public string StartDate { get; set; }
        public string CompleteDate { get; set; }
        public string? Page { get; set; } = string.Empty;
        public string StartStepNumber { get; set; }
        public string StartTask { get; set; }
        public string StartTechnician { get; set; }
        public string StartDateParameter { get; set; }
        public string IncrementValue { get; set; }
        public string LastRow { get; set; }

        public TravelerMPITemplate(string json)
        {
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    JsonConvert.PopulateObject(json, this);
                }
                catch (JsonException ex)
                {

                }
            }
        }
    }

    public class TravelerMTITemplate
    {
        public string AssemblyPartNumber { get; set; }
        public string Description { get; set; }
        public string DocumentControlNumber { get; set; }
        public string Reference { get; set; }
        public string RevisionNumber { get; set; }
        public string WorkOrder { get; set; }
        public string SerialNumber { get; set; }
        public string StartDate { get; set; }
        public string CompleteDate { get; set; }
        public string? Page { get; set; } = string.Empty;
        public string StartStepNumber { get; set; }
        public string StartTask { get; set; }
        public string StartTechnicianAndDate { get; set; }
        public string StartParameter { get; set; }
        public string IncrementValue { get; set; }
        public string LastRow { get; set; }
    }
}
