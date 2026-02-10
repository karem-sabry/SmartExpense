using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartExpense.Core.Entities;

namespace SmartExpense.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<RecurringTransaction> RecurringTransactions => Set<RecurringTransaction>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // User Configuration
        builder.Entity<User>(entity =>
        {
            entity.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(u => u.LastName).HasMaxLength(100).IsRequired();
            entity.Property(u => u.RefreshToken).HasMaxLength(500);
        });

        // Category Configuration
        builder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).HasMaxLength(100).IsRequired();
            entity.Property(c => c.Icon).HasMaxLength(50);
            entity.Property(c => c.Color).HasMaxLength(7);
            entity.Property(c => c.CreatedBy).HasMaxLength(100);
            entity.Property(c => c.UpdatedBy).HasMaxLength(100);


            entity.HasOne(c => c.User)
                .WithMany(u => u.Categories)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        builder.Entity<Transaction>(entity =>
        {
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Description).HasMaxLength(200).IsRequired();
            entity.Property(t => t.Amount).HasPrecision(18, 2).IsRequired();
            entity.Property(t => t.Notes).HasMaxLength(500);
            entity.Property(t => t.CreatedBy).HasMaxLength(100);
            entity.Property(t => t.UpdatedBy).HasMaxLength(100);


            // Relationships
            entity.HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(t => t.Category)
                .WithMany()
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Budget>(entity =>
        {
            entity.HasKey(b => b.Id);

            entity.Property(b => b.Amount).HasPrecision(18, 2).IsRequired();
            entity.Property(b => b.CreatedBy).HasMaxLength(100);
            entity.Property(b => b.UpdatedBy).HasMaxLength(100);


            entity.HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(b => b.Category)
                .WithMany()
                .HasForeignKey(b => b.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<RecurringTransaction>(entity =>
        {
            entity.HasKey(r => r.Id);

            entity.Property(r => r.Description).HasMaxLength(200).IsRequired();
            entity.Property(r => r.Amount).HasPrecision(18, 2).IsRequired();
            entity.Property(r => r.Notes).HasMaxLength(500);
            entity.Property(r => r.CreatedBy).HasMaxLength(100);
            entity.Property(r => r.UpdatedBy).HasMaxLength(100);

            entity.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.Category)
                .WithMany()
                .HasForeignKey(r => r.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}