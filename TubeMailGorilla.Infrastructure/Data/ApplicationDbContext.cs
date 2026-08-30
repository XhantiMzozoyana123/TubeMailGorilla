using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TubeMailGorilla.Domain.Entities;
using TubeMailGorilla.Infrastructure.Models;

namespace TubeMailGorilla.Infrastructure.Data;

/// <summary>
/// Entity Framework Core database context that extends ASP.NET Core Identity.
/// Lives in the Infrastructure layer so the Domain layer remains persistence-agnostic.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Subscription>(entity =>
        {
            entity.ToTable("Subscriptions");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.UserId).HasMaxLength(255).IsRequired();
            entity.Property(s => s.PayPalOrderId).HasMaxLength(64).IsRequired();
            entity.Property(s => s.PayPalCaptureId).HasMaxLength(64);
            entity.Property(s => s.Currency).HasMaxLength(8).IsRequired();
            entity.Property(s => s.Status).HasMaxLength(32).IsRequired();
            entity.Property(s => s.Amount).HasPrecision(12, 2);
            entity.HasIndex(s => s.PayPalOrderId).IsUnique();
            entity.HasIndex(s => s.UserId);
        });
    }
}
