using Finance_Tracker_WPF_API.Core.Models;

namespace Finance_Tracker_WPF_API.Core.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<IEnumerable<Category>> GetByTypeAsync(TransactionType type);
    Task<IEnumerable<Category>> GetAllAsync();
} 