using System.ComponentModel.DataAnnotations;

namespace DMD_Prototype.Models
{
    public class RequestSessionModel
    {
        [Key]
        public int TakeSessionID { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string SWID { get; set; } = string.Empty;


        public RequestSessionModel CreateSessionRequest(string userId, string swId)
        {
            UserId = userId;
            SWID = swId;

            return this;
        }
    }
}
