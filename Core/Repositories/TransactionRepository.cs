using Finance_Tracker_WPF_API.Core.Data;
using Finance_Tracker_WPF_API.Core.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Finance_Tracker_WPF_API.Core.Repositories;

public class TransactionRepository : Repository<Transaction>, ITransactionRepository
{
    public TransactionRepository(AppDbContext context) : base(context)
    {
        Log.Debug("TransactionRepository initialized");
    }

    public async Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            Log.Debug("Getting transactions by date range: {StartDate} to {EndDate}", startDate, endDate);
            var transactions = await _dbSet
                .Include(t => t.Category)
                .Where(t => t.Date >= startDate && t.Date <= endDate)
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            Log.Debug("Retrieved {Count} transactions", transactions.Count);
            Log.Information("Transactions retrieved successfully");
            return transactions;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting transactions by date range: {StartDate} to {EndDate}", startDate, endDate);
            throw;
        }
    }

    public async Task<IEnumerable<Transaction>> GetByCategoryAsync(int categoryId)
    {
        try
        {
            Log.Debug("Getting transactions by category: {CategoryId}", categoryId);
            var transactions = await _dbSet
                .Include(t => t.Category)
                .Where(t => t.CategoryId == categoryId)
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            Log.Debug("Retrieved {Count} transactions for category {CategoryId}", transactions.Count, categoryId);
            Log.Information("Transactions retrieved successfully");
            return transactions;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting transactions by category: {CategoryId}", categoryId);
            throw;
        }
    }

    public async Task<IEnumerable<Transaction>> GetByTypeAsync(TransactionType type)
    {
        try
        {
            Log.Debug("Getting transactions by type: {TransactionType}", type);
            var transactions = await _dbSet
                .Include(t => t.Category)
                .Where(t => t.Type == type)
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            Log.Debug("Retrieved {Count} transactions for type {TransactionType}", transactions.Count, type);
            Log.Information("Transactions retrieved successfully");
            return transactions;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting transactions by type: {TransactionType}", type);
            throw;
        }
    }

    public async Task<decimal> GetTotalByTypeAsync(TransactionType type, DateTime startDate, DateTime endDate)
    {
        try
        {
            Log.Debug("Getting total for type {TransactionType} between {StartDate} and {EndDate}", type, startDate, endDate);
            var transactions = await _dbSet
                .Where(t => t.Type == type && t.Date >= startDate && t.Date <= endDate)
                .ToListAsync();
            
            var total = transactions.Sum(t => t.Amount);
            Log.Debug("Total for {TransactionType}: {Total}", type, total);
            Log.Information("Total calculated successfully");
            return total;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting total for type {TransactionType}", type);
            throw;
        }
    }

    public async Task<Dictionary<string, decimal>> GetCategoryTotalsAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            Log.Debug("Getting category totals between {StartDate} and {EndDate}", startDate, endDate);
            var transactions = await _dbSet
                .Include(t => t.Category)
                .Where(t => t.Date >= startDate && t.Date <= endDate)
                .ToListAsync();

            var totals = transactions
                .GroupBy(t => t.Category!.Name)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

            Log.Debug("Retrieved totals for {CategoryCount} categories", totals.Count);
            Log.Information("Category totals calculated successfully");
            return totals;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting category totals");
            throw;
        }
    }

    public override async Task<Transaction> AddAsync(Transaction entity)
    {
        try
        {
            Log.Debug("Adding new transaction: {TransactionType}, Amount={Amount}, Description={Description}",
                entity.Type, entity.Amount, entity.Description);
            var result = await base.AddAsync(entity);
            Log.Information("Transaction added successfully");
            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error adding transaction");
            throw;
        }
    }

    public override async Task UpdateAsync(Transaction entity)
    {
        try
        {
            Log.Debug("Updating transaction: Id={TransactionId}, Amount={Amount}, Description={Description}",
                entity.Id, entity.Amount, entity.Description);
            await base.UpdateAsync(entity);
            Log.Information("Transaction updated successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating transaction: Id={TransactionId}", entity.Id);
            throw;
        }
    }

    public override async Task DeleteAsync(Transaction entity)
    {
        try
        {
            Log.Debug("Deleting transaction: Id={TransactionId}", entity.Id);
            await base.DeleteAsync(entity);
            Log.Information("Transaction deleted successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error deleting transaction: Id={TransactionId}", entity.Id);
            throw;
        }
    }

    public override async Task SaveChangesAsync()
    {
        try
        {
            Log.Debug("Saving changes to database");
            await base.SaveChangesAsync();
            Log.Information("Changes saved successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error saving changes to database");
            throw;
        }
    }
}