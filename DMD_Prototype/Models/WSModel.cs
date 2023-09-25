using System.ComponentModel.DataAnnotations;

namespace DMD_Prototype.Models
{
    public class WSModel
    {
        [Key]
        public int WSID { get; set; }

        public byte[] WorkmanshipStandardDoc { get; set; }
        public string RevNo { get; set; }
    }
}
