using Finance_Tracker_WPF_API.Core.Models;

namespace Finance_Tracker_WPF_API.Core.Repositories;

public interface ITransactionRepository : IRepository<Transaction>
{
    Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Transaction>> GetByCategoryAsync(int categoryId);
    Task<IEnumerable<Transaction>> GetByTypeAsync(TransactionType type);
    Task<decimal> GetTotalByTypeAsync(TransactionType type, DateTime startDate, DateTime endDate);
    Task<Dictionary<string, decimal>> GetCategoryTotalsAsync(DateTime startDate, DateTime endDate);
}