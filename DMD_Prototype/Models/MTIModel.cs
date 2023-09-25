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

        // Conditionals

        public bool ObsoleteStat { get; set; } = false;
        public string Product { get; set; } = string.Empty;
    }
}
