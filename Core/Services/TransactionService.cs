using Finance_Tracker_WPF_API.Core.Models;
using Finance_Tracker_WPF_API.Core.Patterns;
using Finance_Tracker_WPF_API.Core.Repositories;

namespace Finance_Tracker_WPF_API.Core.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ITransactionFactory _transactionFactory;
    private readonly ReportBuilder _reportBuilder;

    public TransactionService(
        ITransactionRepository transactionRepository,
        ITransactionFactory transactionFactory)
    {
        _transactionRepository = transactionRepository;
        _transactionFactory = transactionFactory;
        _reportBuilder = new ReportBuilder();
    }

    public async Task<Transaction> CreateTransactionAsync(decimal amount, string description, int categoryId, TransactionType type, string? note = null)
    {
        var transaction = type == TransactionType.Income
            ? _transactionFactory.CreateIncomeTransaction(amount, description, categoryId, note)
            : _transactionFactory.CreateExpenseTransaction(amount, description, categoryId, note);

        await _transactionRepository.AddAsync(transaction);
        await _transactionRepository.SaveChangesAsync();
        return transaction;
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        startDate ??= DateTime.MinValue;
        endDate ??= DateTime.MaxValue;
        return await _transactionRepository.GetByDateRangeAsync(startDate.Value, endDate.Value);
    }

    public async Task<Transaction?> GetTransactionByIdAsync(int id)
    {
        return await _transactionRepository.GetByIdAsync(id);
    }

    public async Task UpdateTransactionAsync(Transaction transaction)
    {
        await _transactionRepository.UpdateAsync(transaction);
        await _transactionRepository.SaveChangesAsync();
    }

    public async Task DeleteTransactionAsync(int id)
    {
        var transaction = await _transactionRepository.GetByIdAsync(id);
        if (transaction != null)
        {
            await _transactionRepository.DeleteAsync(transaction);
            await _transactionRepository.SaveChangesAsync();
        }
    }

    public async Task<decimal> GetBalanceAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        startDate ??= DateTime.MinValue;
        endDate ??= DateTime.MaxValue;

        var income = await _transactionRepository.GetTotalByTypeAsync(TransactionType.Income, startDate.Value, endDate.Value);
        var expenses = await _transactionRepository.GetTotalByTypeAsync(TransactionType.Expense, startDate.Value, endDate.Value);

        return income - expenses;
    }

    public async Task<Dictionary<string, decimal>> GetCategoryTotalsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        startDate ??= DateTime.MinValue;
        endDate ??= DateTime.MaxValue;
        return await _transactionRepository.GetCategoryTotalsAsync(startDate.Value, endDate.Value);
    }

    public async Task<Report> GenerateReportAsync(DateTime startDate, DateTime endDate, string? notes = null)
    {
        var transactions = await _transactionRepository.GetByDateRangeAsync(startDate, endDate);
        var categoryTotals = await _transactionRepository.GetCategoryTotalsAsync(startDate, endDate);

        return _reportBuilder
            .SetDateRange(startDate, endDate)
            .AddTransactions(transactions)
            .CalculateTotals()
            .CalculateCategoryTotals()
            .AddNotes(notes ?? string.Empty)
            .Build();
    }
} 