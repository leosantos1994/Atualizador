using Microsoft.EntityFrameworkCore;
using Updater.Models;

namespace Updater.Repository
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }

        public DbSet<MidModel.ServiceModel> Service { get; set; }
        public DbSet<Models.Version> Version { get; set; }
        public DbSet<VersionFile> VersionFile { get; set; }
        public DbSet<Client> Client { get; set; }
        public DbSet<User> User { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
          
        }
    }
}
