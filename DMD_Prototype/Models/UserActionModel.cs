using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace DMD_Prototype.Models
{
    public class UserActionModel
    {
        [Key]
        public int ActionId { get; set; }
        public string Action { get; set; }
        public string Originator { get; set; }
        public DateTime Date { get; set; }


        public UserActionModel CreateAction(string action, string originator, DateTime date)
        {
            Action = action;
            Originator = originator;
            Date = date;

            return this;
        }
    }
}
