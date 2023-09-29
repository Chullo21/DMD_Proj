using System.ComponentModel.DataAnnotations;

namespace DMD_Prototype.Models
{
    public class PauseWorkModel
    {
        [Key]
        public int PWID { get; set; }
        public string SessionID { get; set; }
        public DateTime PauseDT { get; set; } = DateTime.Now;
        public string? PauseReason { get; set; }
        public DateTime? RestartDT { get; set; }
        public string Technician { get; set; }

        public PauseWorkModel SetPause(string sessionID, string reason, string tech)
        {
            SessionID = sessionID;
            PauseReason = reason;
            Technician = tech;

            return this;
        }

        public PauseWorkModel UpdateTech(PauseWorkModel pWM, string worker)
        {
            pWM.Technician = worker;

            return pWM;
        }

        public PauseWorkModel ContinuePausedSession()
        {
            this.RestartDT = DateTime.Now;
            return this;
        }
    }
}
