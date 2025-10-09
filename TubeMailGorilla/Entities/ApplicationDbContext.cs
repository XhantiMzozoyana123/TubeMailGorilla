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
                optionsBuilder.UseSqlServer("workstation id=tmgonline.mssql.somee.com;packet size=4096;user id=xm7cj1dvo2_SQLLogin_1;pwd=y7vyxtrvdt;data source=tmgonline.mssql.somee.com;persist security info=False;initial catalog=tmgonline;TrustServerCertificate=True");
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
