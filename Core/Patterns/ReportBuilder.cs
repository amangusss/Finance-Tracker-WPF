using Finance_Tracker_WPF_API.Core.Models;

namespace Finance_Tracker_WPF_API.Core.Patterns;

public class ReportBuilder
{
    private readonly Report _report = new();

    public ReportBuilder SetDateRange(DateTime startDate, DateTime endDate)
    {
        _report.StartDate = startDate;
        _report.EndDate = endDate;
        return this;
    }

    public ReportBuilder AddTransactions(IEnumerable<Transaction> transactions)
    {
        _report.Transactions.AddRange(transactions);
        return this;
    }

    public ReportBuilder CalculateTotals()
    {
        _report.TotalIncome = _report.Transactions
            .Where(t => t.Type == TransactionType.Income)
            .Sum(t => t.Amount);

        _report.TotalExpenses = _report.Transactions
            .Where(t => t.Type == TransactionType.Expense)
            .Sum(t => t.Amount);

        return this;
    }

    public ReportBuilder CalculateCategoryTotals()
    {
        _report.CategoryTotals = _report.Transactions
            .GroupBy(t => t.Category?.Name ?? "Unknown")
            .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

        return this;
    }

    public ReportBuilder AddNotes(string notes)
    {
        _report.Notes = notes;
        return this;
    }

    public Report Build()
    {
        return _report;
    }
} 