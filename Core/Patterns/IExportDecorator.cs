using Finance_Tracker_WPF_API.Core.Models;

namespace Finance_Tracker_WPF_API.Core.Patterns;

public interface IExportDecorator
{
    void Export(IEnumerable<Transaction> transactions, string filePath);
    string GetFileExtension();
} 