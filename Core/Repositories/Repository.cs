using Finance_Tracker_WPF_API.Core.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Finance_Tracker_WPF_API.Core.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
        Log.Debug("Repository<{EntityType}> initialized", typeof(T).Name);
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        try
        {
            Log.Debug("Getting {EntityType} by Id={Id}", typeof(T).Name, id);
            return await _dbSet.FindAsync(id);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting {EntityType} by Id={Id}", typeof(T).Name, id);
            throw;
        }
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        try
        {
            Log.Debug("Getting all {EntityType} entities", typeof(T).Name);
            var entities = await _dbSet.ToListAsync();
            Log.Debug("Retrieved {Count} {EntityType} entities", entities.Count, typeof(T).Name);
            return entities;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting all {EntityType} entities", typeof(T).Name);
            throw;
        }
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        try
        {
            Log.Debug("Adding new {EntityType} entity", typeof(T).Name);
            var entry = await _dbSet.AddAsync(entity);
            await SaveChangesAsync();
            Log.Debug("Successfully added {EntityType} entity", typeof(T).Name);
            return entry.Entity;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error adding {EntityType} entity", typeof(T).Name);
            throw;
        }
    }

    public virtual async Task UpdateAsync(T entity)
    {
        try
        {
            Log.Debug("Updating {EntityType} entity", typeof(T).Name);
            _dbSet.Update(entity);
            await SaveChangesAsync();
            Log.Debug("Successfully updated {EntityType} entity", typeof(T).Name);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating {EntityType} entity", typeof(T).Name);
            throw;
        }
    }

    public virtual async Task DeleteAsync(T entity)
    {
        try
        {
            Log.Debug("Deleting {EntityType} entity", typeof(T).Name);
            _dbSet.Remove(entity);
            await SaveChangesAsync();
            Log.Debug("Successfully deleted {EntityType} entity", typeof(T).Name);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error deleting {EntityType} entity", typeof(T).Name);
            throw;
        }
    }

    public virtual async Task SaveChangesAsync()
    {
        try
        {
            Log.Debug("Saving changes for {EntityType}", typeof(T).Name);
            await _context.SaveChangesAsync();
            Log.Debug("Successfully saved changes for {EntityType}", typeof(T).Name);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error saving changes for {EntityType}", typeof(T).Name);
            throw;
        }
    }
}