using Finance_Tracker_WPF_API.Core.Models;
using System.Collections.ObjectModel;

namespace Finance_Tracker_WPF_API.Core.Patterns;

public class ReportBuilder
{
    private readonly Report _report;

    public ReportBuilder()
    {
        _report = new Report();
    }

    public ReportBuilder WithDateRange(DateTime startDate, DateTime endDate)
    {
        _report.StartDate = startDate;
        _report.EndDate = endDate;
        return this;
    }

    public ReportBuilder WithTransactions(IEnumerable<Transaction> transactions)
    {
        foreach (var transaction in transactions)
        {
            _report.Transactions.Add(transaction);
        }

        var categoryTotals = transactions
            .GroupBy(t => t.Category.Name)
            .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

        _report.CategoryTotals = categoryTotals;

        _report.Balance = transactions.Sum(t => t.Amount);

        return this;
    }

    public ReportBuilder WithNotes(string notes)
    {
        _report.Notes = notes;
        return this;
    }

    public Report Build()
    {
        _report.GeneratedDate = DateTime.UtcNow;
        return _report;
    }
}