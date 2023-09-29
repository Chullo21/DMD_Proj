﻿using System.ComponentModel.DataAnnotations;

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
        public string Category { get; set; } = string.Empty;
        public string RC { get; set; } = string.Empty;
        public string CA { get; set; } = string.Empty;
        public string InterimDoc { get; set; } = string.Empty;
        public DateTime? IDTCD { get; set; }
        public string IDStatus { get; set; } = "For Validation";
        public string StandardizedDoc { get; set; } = string.Empty;
        public DateTime? SDTCD { get; set; }
        public string SDStatus { get; set; } = "OPEN";
        public string Validator { get; set; } = string.Empty;
        public string PLIDStatus { get; set; } = "OPEN";
        public string PLSDStatus { get; set; } = "OPEN";
        public string PLRemarks { get; set; } = string.Empty;

        public ProblemLogModel CreatePL(string plNo, DateTime logDate, string workWeek, string affectedDoc,
            string product, string pnDn, string desc, string problem, string reporter)
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

            return this;
        }
    }
}
