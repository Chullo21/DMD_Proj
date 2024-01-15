using System.ComponentModel.DataAnnotations;

namespace DMD_Prototype.Models
{
    public class SerialNumberModel
    {
        [Key]
        public int SNID { get; set; }

        public string SessionId { get; set; }
        public string SerialNumber { get; set; }

        public SerialNumberModel SubmitSerialNumber(string serialNumber, string sessionId)
        {
            SerialNumber = serialNumber;
            SessionId = sessionId;

            return this;
        }
    }
}
