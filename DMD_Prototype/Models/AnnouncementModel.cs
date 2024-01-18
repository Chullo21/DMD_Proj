using System.ComponentModel.DataAnnotations;

namespace DMD_Prototype.Models
{
    public class AnnouncementModel
    {
        [Key]
        public int AnnouncementID { get; set; }
        public string AnnouncementMessage { get; set; }
        public string AnnouncementCreator { get; set; }
        public DateTime DateAnnounced { get; set; } = DateTime.Now;

        public AnnouncementModel CreateAnnouncement(string message, string announcer)
        {
            AnnouncementMessage = message;
            AnnouncementCreator = announcer;
            return this;
        }
    }
}
