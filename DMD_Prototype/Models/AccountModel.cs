using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace DMD_Prototype.Models
{
    public class AccountModel
    {
        [Key]
        public int AccID { get; set; }

        public string UserID { get; set; } = string.Empty;

        public string AccName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public DateTime DateCreated { get; set; } = DateTime.Now;

        public AccountModel CreateUser(string accname, string email, string username, string password)
        {
            Guid newGuid = Guid.NewGuid();

            AccountModel newAccount = new AccountModel();
            {
                newAccount.UserID = accname + newGuid.ToString()[..8];
                newAccount.AccName = accname;
                newAccount.Email = email;
                newAccount.Username = username;
                newAccount.Password = password;
                newAccount.DateCreated = DateTime.Now;
            }
            
            return newAccount;
        }

    }

    public enum Roles
    {
        ADMIN,
        ORIGINATOR,
        USER,
        PL_INTERVENOR
    }
}
