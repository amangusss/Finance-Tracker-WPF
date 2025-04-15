using Finance_Tracker_WPF_API.Core.Models;
using Finance_Tracker_WPF_API.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Finance_Tracker_WPF_API.Core.Repositories;

public class TransactionRepository : Repository<Transaction>, ITransactionRepository
{
    public TransactionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(t => t.Category)
            .Where(t => t.Date >= startDate && t.Date <= endDate)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Transaction>> GetByCategoryAsync(int categoryId)
    {
        return await _dbSet
            .Include(t => t.Category)
            .Where(t => t.CategoryId == categoryId)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Transaction>> GetByTypeAsync(TransactionType type)
    {
        return await _dbSet
            .Include(t => t.Category)
            .Where(t => t.Type == type)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalByTypeAsync(TransactionType type, DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(t => t.Type == type && t.Date >= startDate && t.Date <= endDate)
            .SumAsync(t => t.Amount);
    }

    public async Task<Dictionary<string, decimal>> GetCategoryTotalsAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(t => t.Category)
            .Where(t => t.Date >= startDate && t.Date <= endDate)
            .GroupBy(t => t.Category!.Name)
            .ToDictionaryAsync(g => g.Key, g => g.Sum(t => t.Amount));
    }
} 