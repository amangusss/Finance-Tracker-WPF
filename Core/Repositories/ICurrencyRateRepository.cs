using Finance_Tracker_WPF_API.Core.Models;

namespace Finance_Tracker_WPF_API.Core.Repositories;

public interface ICurrencyRateRepository : IRepository<CurrencyRate>
{
    Task<CurrencyRate?> GetLatestRateAsync(string fromCurrency, string toCurrency);
}