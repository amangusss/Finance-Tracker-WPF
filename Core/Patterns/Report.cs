using Finance_Tracker_WPF_API.Core.Models;

namespace Finance_Tracker_WPF_API.Core.Patterns;

public class Report
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public List<Transaction> Transactions { get; set; } = new();
    public Dictionary<string, decimal> CategoryTotals { get; set; } = new();
    public string? Notes { get; set; }

    public decimal Balance => TotalIncome - TotalExpenses;
} 