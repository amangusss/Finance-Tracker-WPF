using Finance_Tracker_WPF_API.Core.Models;

namespace Finance_Tracker_WPF_API.Core.Services;

public interface IExchangeRateService
{
    Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency);
    Task UpdateExchangeRatesAsync();
    Task<IEnumerable<CurrencyRate>> GetLatestRatesAsync(string baseCurrency);
    Task<IEnumerable<string>> GetAvailableCurrenciesAsync();
} 