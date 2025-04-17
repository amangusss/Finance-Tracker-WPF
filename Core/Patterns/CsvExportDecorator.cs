using System.IO;
using Finance_Tracker_WPF_API.Core.Models;

namespace Finance_Tracker_WPF_API.Core.Patterns;

public class CsvExportDecorator : IExportDecorator
{
    public void Export(IEnumerable<Transaction> transactions, string filePath)
    {
        var lines = new List<string>
        {
            "Date,Type,Amount,Description,Category,Note"
        };

        foreach (var transaction in transactions)
        {
            lines.Add($"{transaction.Date:yyyy-MM-dd},{transaction.Type},{transaction.Amount}," +
                     $"\"{transaction.Description}\",\"{transaction.Category?.Name}\",\"{transaction.Note}\"");
        }

        File.WriteAllLines(filePath, lines);
    }

    public string GetFileExtension()
    {
        return AppConfig.Export.CsvExtension;
    }
} 