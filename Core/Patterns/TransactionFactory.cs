using Finance_Tracker_WPF_API.Core.Models;

namespace Finance_Tracker_WPF_API.Core.Patterns;

public class TransactionFactory : ITransactionFactory
{
    public Transaction CreateTransaction(decimal amount, string description, int categoryId, string? note = null)
    {
        return new Transaction
        {
            Amount = amount,
            Description = description,
            CategoryId = categoryId,
            Date = DateTime.Now,
            Note = note
        };
    }

    public Transaction CreateIncomeTransaction(decimal amount, string description, int categoryId, string? note = null)
    {
        var transaction = CreateTransaction(amount, description, categoryId, note);
        transaction.Type = TransactionType.Income;
        return transaction;
    }

    public Transaction CreateExpenseTransaction(decimal amount, string description, int categoryId, string? note = null)
    {
        var transaction = CreateTransaction(amount, description, categoryId, note);
        transaction.Type = TransactionType.Expense;
        return transaction;
    }
} 