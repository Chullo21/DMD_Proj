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

        public DbSet<ModuleModel> ModuleDb { get; set; }

        public DbSet<SerialNumberModel> SerialNumberDb { get; set; }

        public DbSet<RequestSessionModel> RSDb { get; set; }

        public DbSet<UserActionModel> UADb { get; set; }

        public DbSet<AnnouncementModel> AnnouncementDb { get; set;}
    }
}
