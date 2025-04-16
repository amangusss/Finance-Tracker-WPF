using Finance_Tracker_WPF_API.Core.Models;

namespace Finance_Tracker_WPF_API.Core.Patterns;

public interface ITransactionFactory
{
    Transaction CreateTransaction(decimal amount, string description, int categoryId, TransactionType type, string? note = null);
    Transaction CreateIncomeTransaction(decimal amount, string description, int categoryId, string? note = null);
    Transaction CreateExpenseTransaction(decimal amount, string description, int categoryId, string? note = null);
} 