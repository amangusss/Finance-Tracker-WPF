namespace Finance_Tracker_WPF_API.UI.ViewModels;

public class TransactionDisplayDTO
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public decimal AmountInSelectedCurrency { get; set; }
    public string Currency { get; set; } = "";
    public DateTime Date { get; set; }
    public string Description { get; set; } = "";
    public string Type { get; set; } = "";
    public string CategoryName { get; set; } = "";
    public string Note { get; set; } = "";
}
