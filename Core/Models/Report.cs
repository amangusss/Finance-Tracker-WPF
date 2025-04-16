using System.Collections.ObjectModel;

namespace Finance_Tracker_WPF_API.Core.Models;

public class Report
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Balance { get; set; }
    public Dictionary<string, decimal> CategoryTotals { get; set; } = new();
    public ObservableCollection<Transaction> Transactions { get; set; } = new();
    public string? Notes { get; set; }
    public DateTime GeneratedDate { get; set; }
}
