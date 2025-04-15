using Finance_Tracker_WPF_API.Core;
using Finance_Tracker_WPF_API.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Finance_Tracker_WPF_API.Infrastructure.Database;

public class ApplicationDbContext : DbContext
{
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<CurrencyRate> CurrencyRates { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(AppConfig.Database.ConnectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var id = 1;
        var expenseCategories = AppConfig.Categories.DefaultExpenseCategories
            .Select(name => new Category
            {
                Id = id++,
                Name = name,
                Type = TransactionType.Expense
            });

        var incomeCategories = AppConfig.Categories.DefaultIncomeCategories
            .Select(name => new Category
            {
                Id = id++,
                Name = name,
                Type = TransactionType.Income
            });

        modelBuilder.Entity<Category>().HasData(expenseCategories.Concat(incomeCategories));
    }
} 