using System.ComponentModel.DataAnnotations;

namespace DMD_Prototype.Models
{
    public class MTIModel
    {
        [Key]
        public int MTIID { get; set; }
        public byte[]? Documnet1 { get; set; }
        public byte[]? Documnet2 { get; set; }
        public byte[]? Documnet3 { get; set; }
        public byte[]? Documnet4 { get; set; }
        public string Numberchuchu { get; set; }
    }
}
