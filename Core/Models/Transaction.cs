using System.ComponentModel.DataAnnotations;

namespace Finance_Tracker_WPF_API.Core.Models;

public class Transaction
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public decimal Amount { get; set; }
    
    [Required]
    [MaxLength(10)]
    public string Currency { get; set; } = "USD";
    
    [Required]
    public DateTime Date { get; set; }
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public TransactionType Type { get; set; }
    
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    
    public string? Note { get; set; }
}

public enum TransactionType
{
    Income,
    Expense
}