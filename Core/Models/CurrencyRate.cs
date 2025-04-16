using System.ComponentModel.DataAnnotations;

namespace Finance_Tracker_WPF_API.Core.Models;

public class CurrencyRate
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(3)]
    public string FromCurrency { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(3)]
    public string ToCurrency { get; set; } = string.Empty;
    
    [Required]
    public decimal Rate { get; set; }
    
    [Required]
    public DateTime Date { get; set; }
} 