using DMD_Prototype.Data;
using DMD_Prototype.Models;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Controls;
using System.Reflection;

namespace DMD_Prototype.Controllers
{
    public class DocGeneratorController : Controller
    {
        private readonly string userDir = "D:\\jtoledo\\Desktop\\DMD_SessionFolder";       
        //private readonly string templateDir = "D:\\jtoledo\\Desktop\\Word\\ATS_DMD_TEMPLATES\\TravelerP1.xlsx";
        private readonly string userTravName = "Traveler.xlsx";

        private readonly AppDbContext _Db;
        private readonly List<MTIModel> _mtiModel;
        private readonly List<StartWorkModel> _swModel;

        // process vars

        private string startDate;
        private string endDate;

        public DocGeneratorController(AppDbContext _context)
        {
            _Db = _context;
            _mtiModel = _Db.MTIDb.ToList();
            _swModel = _Db.StartWorkDb.ToList();
        }


        public void InitiateTravelerGeneration(string sessionId)
        {
            List<FromTraveler> fT = TravValueGetter(sessionId);
            int rowCount = 10;
            MTIModel mtiDet = GetDocDetails(sessionId);

            using(ExcelPackage package =  new ExcelPackage(TravTemplateGetter()))
            {
                var ws = package.Workbook.Worksheets[0];

                ws.Cells[4, 3].Value = mtiDet.AssemblyPN;
                ws.Cells[5, 3].Value = mtiDet.AssemblyDesc;
                ws.Cells[3, 5].Value = mtiDet.DocumentNumber;
                ws.Cells[4, 6].Value = mtiDet.RevNo;
                ws.Cells[6, 3].Value = startDate;
                ws.Cells[7, 3].Value = endDate;

                foreach (var f in fT)
                {
                    ws.Cells[rowCount, 1].Value = f.StepNo;
                    ws.Cells[rowCount, 2].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    ws.Cells[rowCount, 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    ws.Cells[rowCount, 2].Value = f.Task;
                    ws.Cells[rowCount, 2].Style.WrapText = true;
                    ws.Cells[rowCount, 8].Value = f.Technician + " || ";
                    ws.Cells[rowCount, 8].Value += f.Date;

                    if (f.Task.Length > 30)
                    {
                        ws.Row(rowCount).Height = f.Task.Length - f.Task.Length/2;
                    }

                    if (!string.IsNullOrEmpty(f.First))
                    {
                        ws.Cells[rowCount, 5].Value = f.First + " || ";
                        ws.Cells[rowCount, 5].Value += f.Second + " || ";
                        ws.Cells[rowCount, 5].Value += f.Third;
                    }
                    else
                    {
                        ws.Cells[rowCount, 5].Value += f.Single;
                    }

                    ws.Cells[rowCount, 1, rowCount, 1].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    ws.Cells[rowCount, 2, rowCount, 4].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    ws.Cells[rowCount, 5, rowCount, 6].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    ws.Cells[rowCount, 8, rowCount, 9].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);


                    if (CheckPageSetup(ws))
                    {
                        ws.DeleteRow(rowCount - 1);
                    }

                    rowCount++;
                }
                    
                package.SaveAs(Path.Combine("D:\\jtoledo\\Desktop\\Word", "test.xlsx"));
            }
        }

        private bool CheckPageSetup(ExcelWorksheet ws)
        {
            int rowCount = ws.Dimension.Rows;
            int columnCount = ws.Dimension.Columns;

            double a4RowCount = 297 / 0.0393701;
            double a4ColumnCount = 210 / 0.0393701;

            return rowCount > a4RowCount || columnCount > a4ColumnCount;
        }

        public Stream TravTemplateGetter()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            string resourceName = "DMD_Prototype.wwwroot.Common.Templates.TravelerP1.xlsx";
            Stream stream = assembly.GetManifestResourceStream(resourceName);

            return stream;
        }

        private MTIModel GetDocDetails(string sessionId)
        {
            StartWorkModel sw = _swModel.FirstOrDefault(j => j.SessionID == sessionId);

            startDate = sw.StartDate.ToShortDateString();
            endDate = sw.FinishDate.Value.ToShortDateString();

            MTIModel mti = _mtiModel.FirstOrDefault(j => j.DocumentNumber == sw.DocNo);

            return mti;
        }

        private List<FromTraveler> TravValueGetter(string sessionID)
        {
            List<FromTraveler> res = new List<FromTraveler>();
            int rowCount = 1;

            using(ExcelPackage package = new ExcelPackage(Path.Combine(userDir, sessionID, userTravName)))
            {
                var ws = package.Workbook.Worksheets[0];

                do
                {
                    if (ws.Cells[rowCount, 2].Value == null)
                    {
                        break;
                    }

                    FromTraveler ft = new FromTraveler();
                    {
                        ft.StepNo = ws.Cells[rowCount, 1].Value == null ? "" : ws.Cells[rowCount, 1].Value.ToString();
                        ft.Task = ws.Cells[rowCount, 2].Value.ToString();
                        ft.First = ws.Cells[rowCount, 4].Value == null ? "" : ws.Cells[rowCount, 4].Value.ToString();
                        ft.Second = ws.Cells[rowCount, 5].Value == null ? "" : ws.Cells[rowCount, 5].Value.ToString();
                        ft.Third = ws.Cells[rowCount, 6].Value == null ? "" : ws.Cells[rowCount, 6].Value.ToString();
                        ft.Single = ws.Cells[rowCount, 7].Value == null ? "" : ws.Cells[rowCount, 7].Value.ToString();
                        ft.Technician = ws.Cells[rowCount, 8].Value == null ? "" : ws.Cells[rowCount, 8].Value.ToString();
                        ft.Date = ws.Cells[rowCount, 9].Value == null ? "" : ws.Cells[rowCount, 9].Value.ToString();
                    }

                    res.Add(ft);

                    rowCount++;
                } while (true);
            }

            return res;
        }
    }

    class FromTraveler
    {
        public string? StepNo { get; set; }
        public string Task { get; set; } = string.Empty;
        public string? First { get; set; }
        public string? Second { get; set; }
        public string? Third { get; set; }
        public string? Single { get; set; }
        public string Technician { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
    }
}
