using Finance_Tracker_WPF_API.Core.Data;
using Finance_Tracker_WPF_API.Core.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Finance_Tracker_WPF_API.Core.Repositories;

public class CurrencyRateRepository : Repository<CurrencyRate>, ICurrencyRateRepository
{
    public CurrencyRateRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<CurrencyRate?> GetLatestRateAsync(string fromCurrency, string toCurrency)
    {
        try
        {
            Log.Debug("Getting latest currency rate for {FromCurrency} to {ToCurrency}", fromCurrency, toCurrency);
            
            var rate = await _context.Set<CurrencyRate>()
                .Where(r => r.FromCurrency == fromCurrency && r.ToCurrency == toCurrency)
                .OrderByDescending(r => r.Date)
                .FirstOrDefaultAsync();

            if (rate == null)
            {
                Log.Warning("No currency rate found for {FromCurrency} to {ToCurrency}", fromCurrency, toCurrency);
            }
            else
            {
                Log.Debug("Found currency rate: {Rate}", rate.Rate);
            }

            return rate;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting latest currency rate for {FromCurrency} to {ToCurrency}", fromCurrency, toCurrency);
            throw;
        }
    }
}
