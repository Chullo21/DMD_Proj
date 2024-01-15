using System.ComponentModel.DataAnnotations;

namespace DMD_Prototype.Models
{
    public class ModuleModel
    {
        [Key]
        public int ModuleID { get; set; }
        public string SessionID { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;

        //public string SerialNo { get; set; } = string.Empty;
        public string WorkOrder { get; set; } = string.Empty;

        public ModuleModel CreateModule(string sessionId, string module, string wO)
        {
            SessionID = sessionId;
            Module = module;
            //SerialNo = serialNo;
            WorkOrder = wO;

            return this;
        }
    }

    
}
