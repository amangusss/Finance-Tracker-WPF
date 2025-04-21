using Finance_Tracker_WPF_API.Core.Models;
using Finance_Tracker_WPF_API.Core.Repositories;
using Serilog;

namespace Finance_Tracker_WPF_API.Core.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICategoryRepository _categoryRepository;

    public TransactionService(
        ITransactionRepository transactionRepository,
        ICategoryRepository categoryRepository)
    {
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<Transaction> CreateTransactionAsync(decimal amount, string description, int categoryId, TransactionType type, string? note = null, string currency = "USD")
    {
        Log.Information("Creating new transaction: Amount={Amount}, Description={Description}, CategoryId={CategoryId}, Type={Type}, Currency={Currency}",
            amount, description, categoryId, type, currency);

        var transaction = new Transaction
        {
            Amount = amount,
            Description = description,
            CategoryId = categoryId,
            Type = type,
            Note = note,
            Currency = currency,
            Date = DateTime.Now
        };

        try
        {
            var result = await _transactionRepository.AddAsync(transaction);
            Log.Information("Transaction created successfully. Id={Id}", result.Id);
            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error creating transaction");
            throw;
        }
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            Log.Debug("Retrieving all transactions");
            var transactions = await _transactionRepository.GetAllAsync();
            var result = transactions.OrderByDescending(t => t.Date).ToList();
            Log.Debug("Retrieved {Count} transactions", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving transactions");
            throw;
        }
    }

    public async Task<Transaction?> GetTransactionByIdAsync(int id)
    {
        try
        {
            Log.Debug("Retrieving transaction by id: {Id}", id);
            var transaction = await _transactionRepository.GetByIdAsync(id);
            if (transaction == null)
            {
                Log.Warning("Transaction not found: {Id}", id);
            }
            return transaction;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving transaction by id");
            throw;
        }
    }

    public async Task UpdateTransactionAsync(Transaction transaction)
    {
        try
        {
            Log.Debug("Updating transaction: {Id}", transaction.Id);
            await _transactionRepository.UpdateAsync(transaction);
            Log.Information("Transaction updated successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating transaction");
            throw;
        }
    }

    public async Task DeleteTransactionAsync(int id)
    {
        try
        {
            Log.Debug("Deleting transaction: {Id}", id);
            var transaction = await GetTransactionByIdAsync(id);
            if (transaction != null)
            {
                await _transactionRepository.DeleteAsync(transaction);
                Log.Information("Transaction deleted successfully");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error deleting transaction");
            throw;
        }
    }

    public async Task<decimal> GetBalanceAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            Log.Debug("Calculating balance for period: {StartDate} to {EndDate}", startDate, endDate);
            var transactions = await GetTransactionsAsync(startDate, endDate);
            var balance = transactions.Sum(t => t.Type == TransactionType.Income ? t.Amount : -t.Amount);
            Log.Information("Balance calculated: {Balance}", balance);
            return balance;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error calculating balance");
            throw;
        }
    }

    public async Task<Dictionary<string, decimal>> GetCategoryTotalsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            Log.Debug("Calculating category totals for period: {StartDate} to {EndDate}", startDate, endDate);
            var transactions = await GetTransactionsAsync(startDate, endDate);

            var categoryTotals = transactions
                .GroupBy(t => t.Category?.Name ?? "Uncategorized")
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(t => t.Type == TransactionType.Income ? t.Amount : -t.Amount)
                );

            Log.Information("Category totals calculated for {Count} categories", categoryTotals.Count);
            return categoryTotals;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error calculating category totals");
            throw;
        }
    }

    public async Task<Report> GenerateReportAsync(DateTime startDate, DateTime endDate, string? notes = null)
    {
        try
        {
            Log.Information("Generating report for period: {StartDate} to {EndDate}", startDate, endDate);
            
            var report = new Report
            {
                StartDate = startDate,
                EndDate = endDate,
                Balance = await GetBalanceAsync(startDate, endDate),
                CategoryTotals = await GetCategoryTotalsAsync(startDate, endDate),
                Notes = notes,
                GeneratedDate = DateTime.Now
            };

            Log.Information("Report generated successfully");
            return report;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error generating report");
            throw;
        }
    }

    public async Task<IEnumerable<Category>> GetCategoriesByTypeAsync(TransactionType type)
    {
        try
        {
            Log.Debug("Retrieving categories for type: {Type}", type);
            var categories = await _categoryRepository.GetByTypeAsync(type);
            Log.Debug("Retrieved {Count} categories", categories.Count());
            return categories;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving categories by type");
            throw;
        }
    }

    public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
    {
        return await _categoryRepository.GetAllAsync();
    }
}