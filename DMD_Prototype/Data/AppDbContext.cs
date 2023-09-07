using DMD_Prototype.Models;
using Microsoft.EntityFrameworkCore;

namespace DMD_Prototype.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<MTIModel> MTIDb { get; set; }

        public DbSet<InstructionModel> InsDb { get; set; }

        public DbSet<DMD_Prototype.Models.AccountModel>? AccountModel { get; set; }
    }
}
