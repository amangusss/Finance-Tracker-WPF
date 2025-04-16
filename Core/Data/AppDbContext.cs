using Finance_Tracker_WPF_API.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Finance_Tracker_WPF_API.Core.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Transaction> Transactions { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<CurrencyRate> CurrencyRates { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Transaction entity
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).IsRequired();
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.Note);

            entity.HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Category entity
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Type).IsRequired();
        });

        // Configure CurrencyRate entity
        modelBuilder.Entity<CurrencyRate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FromCurrency).IsRequired().HasMaxLength(3);
            entity.Property(e => e.ToCurrency).IsRequired().HasMaxLength(3);
            entity.Property(e => e.Rate).IsRequired();
            entity.Property(e => e.Date).IsRequired();
        });

        // Seed initial categories
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Salary", Type = TransactionType.Income },
            new Category { Id = 2, Name = "Freelance", Type = TransactionType.Income },
            new Category { Id = 3, Name = "Investments", Type = TransactionType.Income },
            new Category { Id = 4, Name = "Groceries", Type = TransactionType.Expense },
            new Category { Id = 5, Name = "Utilities", Type = TransactionType.Expense },
            new Category { Id = 6, Name = "Rent", Type = TransactionType.Expense },
            new Category { Id = 7, Name = "Transportation", Type = TransactionType.Expense },
            new Category { Id = 8, Name = "Entertainment", Type = TransactionType.Expense }
        );
    }
}
