using System.ComponentModel.DataAnnotations;

namespace DMD_Prototype.Models
{
    public class ProblemLogModel
    {
        [Key]
        public int PLID { get; set; }
        public string PLNo { get; set; } = string.Empty;
        public DateTime LogDate { get; set; }
        public string WorkWeek { get; set; } = string.Empty;
        public string AffectedDoc { get; set; } = string.Empty;
        public string Product { get; set; } = string.Empty;
        public string PNDN { get; set; } = string.Empty;
        public string Desc { get; set; } = string.Empty;
        public string Problem { get; set; } = string.Empty;
        public string Reporter { get; set; } = string.Empty;

        public string? Category { get; set; } = string.Empty;
        public string? RC { get; set; } = string.Empty;
        public string? CA { get; set; } = string.Empty;
        public string? InterimDoc { get; set; } = string.Empty;
        public DateTime? IDTCD { get; set; }
        public string? IDStatus { get; set; } = string.Empty;
        public string? StandardizedDoc { get; set; } = string.Empty;
        public DateTime? SDTCD { get; set; }
        public string? SDStatus { get; set; } = string.Empty;

        public string? Validator { get; set; } = string.Empty;
        public string? PLIDStatus { get; set; }
        public string? PLSDStatus { get; set; }
        public string? PLRemarks { get; set; } = string.Empty;

        public string Validation { get; set; } = string.Empty;
        public string? OwnerRemarks { get; set; } = string.Empty;

        // Conditionals

        public string DocNo { get; set; } = string.Empty;
        public char? AffectedDocSys { get; set; }

        public ProblemLogModel CreatePL(string plNo, DateTime logDate, string workWeek, string affectedDoc,
            string product, string pnDn, string desc, string problem, string reporter, string docNo)
        {
            PLNo = plNo;
            LogDate = logDate;
            WorkWeek = workWeek;
            AffectedDoc = affectedDoc;
            Product = product;
            PNDN = pnDn;
            Desc = desc;
            Problem = problem;
            Reporter = reporter;
            DocNo = docNo;

            return this;
        }
    }
}
