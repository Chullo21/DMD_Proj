using System.ComponentModel.DataAnnotations;

namespace DMD_Prototype.Models
{
    public class StartWorkModel
    {
        [Key]
        public int SWID { get; set; }
        public string SessionID { get; set; }
        public string DocNo { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime? FinishDate { get; set; }
        public string UserID { get; set; }

        public StartWorkModel CreateSW(string docNo, string userID)
        {
            SessionID = SessionIDGenerator();
            DocNo = docNo;
            UserID = userID;

            return this;
        }

        private string SessionIDGenerator()
        {
            Guid guid = Guid.NewGuid();
            return guid.ToString()[..20];
        }
    }



}
