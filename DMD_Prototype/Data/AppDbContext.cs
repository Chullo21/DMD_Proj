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

        public DbSet<AccountModel> AccountDb { get; set; }

        public DbSet<StartWorkModel> StartWorkDb { get; set; }

        public DbSet<PauseWorkModel> PauseWorkDb { get; set; }

        public DbSet<ProblemLogModel> PLDb { get; set; }
    }
}
