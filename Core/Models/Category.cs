using System.ComponentModel.DataAnnotations;

namespace Finance_Tracker_WPF_API.Core.Models;

public class Category
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public TransactionType Type { get; set; }
    
    public string? Description { get; set; }
    
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
} 