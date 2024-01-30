using System.ComponentModel.DataAnnotations;

namespace DMD_Prototype.Models
{
    public class MTIModel
    {
        [Key]
        public int MTIID { get; set; }
        public string DocType { get; set; }
        public string OriginatorName { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public string AssemblyPN { get; set; } = string.Empty;
        public string AssemblyDesc { get; set; } = string.Empty;
        public string RevNo { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; } = DateTime.Now;

        //Document Statuses

        public char MTPIStatus { get; set; } = 'c';

        // Special columns

        public string? LogsheetDocNo { get; set; }
        public string? LogsheetRevNo { get; set; }

        // Conditionals

        public string AfterTravLog { get; set; }
        public bool ObsoleteStat { get; set; } = false;
        public string Product { get; set; } = string.Empty;

        public bool isDeleted { get; set; } = false;
    }
}
