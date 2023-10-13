﻿using System.ComponentModel.DataAnnotations;

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
        public string? LogType { get; set; }
        public string UserID { get; set; }

        public StartWorkModel CreateSW(string docNo, string userID, string logType)
        {
            SessionID = SessionIDGenerator();
            DocNo = docNo;
            UserID = userID;
            LogType = logType;

            return this;
        }

        private string SessionIDGenerator()
        {
            Guid guid = Guid.NewGuid();
            return guid.ToString()[..20];
        }
    }



}
