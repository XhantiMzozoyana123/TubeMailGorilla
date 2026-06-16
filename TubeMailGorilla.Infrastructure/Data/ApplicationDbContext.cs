using Microsoft.EntityFrameworkCore;
using TubeMailGorillaDomain.Entities;

namespace TubeMailGorillaInfrastructure.Data
{
    public class InfrastructureDbContext : DbContext
    {
        public InfrastructureDbContext(DbContextOptions<InfrastructureDbContext> options) : base(options)
        {
        }

        public InfrastructureDbContext()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=TubeMailGorillaInfrastructure;Trusted_Connection=True;TrustServerCertificate=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Emailer>(entity =>
            {
                entity.HasIndex(e => e.Email);
            });

            modelBuilder.Entity<Blocker>(entity =>
            {
                entity.HasIndex(e => e.Email);
            });

            modelBuilder.Entity<Captions>(entity =>
            {
                entity.HasIndex(e => e.EmailerId);
            });

            modelBuilder.Entity<Icebreaker>(entity =>
            {
                entity.HasIndex(e => e.EmailerId);
            });
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

