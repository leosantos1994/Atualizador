using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Updater.Models;

namespace Updater.Repository
{
    public class AppDBContext : DbContext
    {
        //public AppDBContext()
        //{
        //}
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { Database.SetCommandTimeout(120); }

        public DbSet<MidModel.ServiceModel> Service { get; set; }
        public DbSet<Models.Version> Version { get; set; }
        public DbSet<VersionFile> VersionFile { get; set; }
        public DbSet<Client> Client { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<ClientUser> ClientUser { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
          
        }

  
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlServer("Data Source=192.168.0.187;Database=atualizadorDB;Integrated Security=false;User ID=sa;Password=Sa12345678;Connection Timeout=15;");
        //    base.OnConfiguring(optionsBuilder);
        //}
    }
}
