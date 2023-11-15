using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OfficeOpenXml;

namespace DMD_Prototype.Controllers
{
    public class DocumentController : Controller
    {
        private readonly ISharedFunct ishared;

        public DocumentController(ISharedFunct ishared)
        {
            this.ishared = ishared;
        }

        public ContentResult GetConfigDataForEdit(string sessionId)
        {
            List<ConfigDataForEdit> res = new List<ConfigDataForEdit>();

            using (ExcelPackage package = new ExcelPackage(Path.Combine(ishared.GetPath("userDir"), sessionId, ishared.GetPath("logName"))))
            {
                int page = 0;
                
                int totalPages = package.Workbook.Worksheets.Count;
                var ws = package.Workbook.Worksheets[page];
                int row = 10;

                do
                {
                    if (row >= 49)
                    {
                        page++;
                        ws = package.Workbook.Worksheets[page];
                        row = 10;
                    }

                    if (page + 1 > totalPages || (ws.Cells[row, 1].Value == null && ws.Cells[row, 3].Value == null && ws.Cells[row, 7].Value == null))
                    {
                        break;
                    }

                    ConfigDataForEdit config = new();

                    config.PN = ws.Cells[row, 1].Value == null ? "" : ws.Cells[row, 1].Value.ToString();
                    config.Desc = ws.Cells[row, 3].Value == null ? "" : ws.Cells[row, 3].Value.ToString();
                    config.Parameter = ws.Cells[row, 7].Value == null ? "" : ws.Cells[row, 7].Value.ToString();

                    res.Add(config);

                    row += 3;

                } while (true);
            }

            string jsonContent = JsonConvert.SerializeObject(new { r = res });
            return Content(jsonContent, "application/json");
        }

        public ContentResult SaveTravChanges(string[] Step, string[] Instruction, string[] SinglePara, string[] FirstThreePara, string[] SecondThreePara, string[] ThirdThreePara, bool[] isMerge, string sessionId)
        {
            string filePath = Path.Combine(ishared.GetPath("userDir"), sessionId, ishared.GetPath("userTravName"));

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

        public ContentResult SaveConfigChanges(string[] PN, string[] Desc, string[] Parameter, string sessionId)
        {
            string filePath = Path.Combine(ishared.GetPath("userDir"), sessionId, ishared.GetPath("logName"));

            using (ExcelPackage package = new(filePath))
            {
                int page = 0;
                int indexCounter = 0;
                var ws = package.Workbook.Worksheets[page];

                int row = 10;

                //for (int i = 0; i < PN.Length; i++)
                //{
                //    ws.Cells[row, 1].Value = PN[i];
                //    ws.Cells[row, 3].Value = Desc[i];
                //    ws.Cells[row, 7].Value = Parameter[i];

                //    row += 3;
                //}

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
    }

    public class ConfigDataForEdit
    {
        public string PN { get; set; }
        public string Desc { get; set; }
        public string Parameter { get; set; }
    }
}
