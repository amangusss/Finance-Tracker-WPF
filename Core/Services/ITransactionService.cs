using Finance_Tracker_WPF_API.Core.Models;
using Finance_Tracker_WPF_API.Core.Patterns;

namespace Finance_Tracker_WPF_API.Core.Services;

public interface ITransactionService
{
    Task<Transaction> CreateTransactionAsync(decimal amount, string description, int categoryId, TransactionType type, string? note = null);
    Task<IEnumerable<Transaction>> GetTransactionsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<Transaction?> GetTransactionByIdAsync(int id);
    Task UpdateTransactionAsync(Transaction transaction);
    Task DeleteTransactionAsync(int id);
    Task<decimal> GetBalanceAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<Dictionary<string, decimal>> GetCategoryTotalsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<Report> GenerateReportAsync(DateTime startDate, DateTime endDate, string? notes = null);
} 