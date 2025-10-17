using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TubeMailGorilla.Entities
{
    public class ApplicationDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=YourDatabaseName;Trusted_Connection=True;TrustServerCertificate=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Blocker> Blockers { get; set; }

        public DbSet<Captions> Captions { get; set; }

        public DbSet<Credientals> Credientals { get; set; }

        public DbSet<Commentor> Commentors { get; set; }

        public DbSet<Emailer> Emailers { get; set; }

        public DbSet<Icebreaker> Icebreakers { get; set; }

        public DbSet<Inboxers> Inboxers { get; set; }

        public DbSet<Proxies> Proxies { get; set; }

        public DbSet<Settings> Settings { get; set; }

        public DbSet<Templates> Templates { get; set; }
    }
}
