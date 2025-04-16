using Finance_Tracker_WPF_API.Core.Data;
using Finance_Tracker_WPF_API.Core.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Finance_Tracker_WPF_API.Core.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Category> AddAsync(Category entity)
    {
        try
        {
            Log.Debug("Adding new category: {Name}", entity.Name);
            var result = await _context.Categories.AddAsync(entity);
            await SaveChangesAsync();
            Log.Information("Category added successfully. Id: {Id}", result.Entity.Id);
            return result.Entity;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error adding category");
            throw;
        }
    }

    public async Task DeleteAsync(Category entity)
    {
        try
        {
            Log.Debug("Deleting category: {Id}", entity.Id);
            _context.Categories.Remove(entity);
            await SaveChangesAsync();
            Log.Information("Category deleted successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error deleting category");
            throw;
        }
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        try
        {
            Log.Debug("Retrieving all categories");
            var categories = await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();
            Log.Debug("Retrieved {Count} categories", categories.Count);
            return categories;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving all categories");
            throw;
        }
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        try
        {
            Log.Debug("Retrieving category by id: {Id}", id);
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                Log.Warning("Category not found: {Id}", id);
            }
            return category;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving category by id");
            throw;
        }
    }

    public async Task<IEnumerable<Category>> GetByTypeAsync(TransactionType type)
    {
        try
        {
            Log.Debug("Retrieving categories for type: {Type}", type);
            var categories = await _context.Categories
                .Where(c => c.Type == type)
                .OrderBy(c => c.Name)
                .ToListAsync();
            
            Log.Debug("Retrieved {Count} categories", categories.Count);
            return categories;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving categories by type");
            throw;
        }
    }

    public async Task SaveChangesAsync()
    {
        try
        {
            Log.Debug("Saving changes to database");
            await _context.SaveChangesAsync();
            Log.Debug("Changes saved successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error saving changes to database");
            throw;
        }
    }

    public async Task UpdateAsync(Category entity)
    {
        try
        {
            Log.Debug("Updating category: {Id}", entity.Id);
            _context.Categories.Update(entity);
            await SaveChangesAsync();
            Log.Information("Category updated successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating category");
            throw;
        }
    }
}
