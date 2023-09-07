using System.ComponentModel.DataAnnotations;

namespace DMD_Prototype.Models
{
    public class InstructionModel
    {
        [Key]
        public int InsID { get; set; }
        public byte[] InsPhoto { get; set; }
        public int Page { get; set; }
    }
}
