using Finance_Tracker_WPF_API.Core.Models;

namespace Finance_Tracker_WPF_API.Core.Patterns;

public class TransactionFactory : ITransactionFactory
{
    public Transaction CreateTransaction(decimal amount, string description, int categoryId, TransactionType type, string? note = null)
    {
        return new Transaction
        {
            Amount = type == TransactionType.Expense ? -amount : amount,
            Description = description,
            CategoryId = categoryId,
            Type = type,
            Note = note,
            Date = DateTime.Now
        };
    }

    public Transaction CreateIncomeTransaction(decimal amount, string description, int categoryId, string? note = null)
    {
        var transaction = CreateTransaction(amount, description, categoryId, TransactionType.Income, note);
        return transaction;
    }

    public Transaction CreateExpenseTransaction(decimal amount, string description, int categoryId, string? note = null)
    {
        var transaction = CreateTransaction(amount, description, categoryId, TransactionType.Expense, note);
        return transaction;
    }
} 