using System.Net.Mail;

namespace DMD_Prototype.Models
{
    public class EmailModel
    {
        public string Email { get; set; }
        public string Password { get; set; }

        public EmailModel FirstEmailAccount()
        {
            Email = "atsdmdnr1@gmail.com";
            Password = "kxpiujldjvhbouuh";

            return this;
        }

        public EmailModel SecondEmailAccount()
        {
            Email = "atsdmdnr2@gmail.com";
            Password = "kozjbottligllkyw";

            return this;
        }
    }
}
