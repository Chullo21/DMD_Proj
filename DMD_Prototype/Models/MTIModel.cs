using System.ComponentModel.DataAnnotations;

namespace DMD_Prototype.Models
{
    public class MTIModel
    {
        [Key]
        public int MTIID { get; set; }
        public string OriginatorName { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public string AssemblyPN { get; set; } = string.Empty;
        public string AssemblyDesc { get; set; } = string.Empty;
        public string RevNo { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public string AssemblyDrawing { get; set; } = string.Empty;
        public string BillsOfMaterial { get; set; } = string.Empty;
        public string OnePointLesson { get; set; } = string.Empty;
        public string SchematicDiagram { get; set; } = string.Empty;
        public string WorkmanshipStandard { get; set; } = string.Empty;
        public string PRCO { get; set; } = string.Empty;
        public string Derogation { get; set; } = string.Empty;
        public string EngrMemo { get; set; } = string.Empty;

        // Conditionals

        public bool ObsoleteStat { get; set; } = false;
        public string Product { get; set; }
    }
}
