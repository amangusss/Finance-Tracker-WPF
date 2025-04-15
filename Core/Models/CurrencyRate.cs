using System.ComponentModel.DataAnnotations;

namespace Finance_Tracker_WPF_API.Core.Models;

public class CurrencyRate
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(3)]
    public string BaseCurrency { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(3)]
    public string TargetCurrency { get; set; } = string.Empty;
    
    [Required]
    public decimal Rate { get; set; }
    
    [Required]
    public DateTime LastUpdated { get; set; }
} 