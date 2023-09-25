namespace DMD_Prototype.Models
{
    public class ViewDocModel
    {
        public byte[] MPI { get; set; }
        public byte[] AssyDrawing { get; set; }
        public byte[] BOM { get; set; }
        public byte[] SchematicDiagram { get; set; }

        public List<TravelerModel> traveler { get; set; }

    }
}
