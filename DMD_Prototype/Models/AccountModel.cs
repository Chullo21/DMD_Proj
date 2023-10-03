using System.ComponentModel.DataAnnotations;

namespace DMD_Prototype.Models
{
    public class AccountModel
    {

        [Key]
        public int AccID { get; set; }

        [Required]
        public string UserID { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;

        [Required]
        public string AccName { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Sec { get; set; } = string.Empty;

        [Required]
        public string Dom { get; set; } = string.Empty;

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public DateTime DateCreated { get; set; } = DateTime.Now;

        //Conditionals

        //public string? ProductHandled { get; set; }

    }

    public enum Roles
    {
        ADMIN,
        ORIGINATOR,
        USER,
        PL_INTERVENOR
    }
}
