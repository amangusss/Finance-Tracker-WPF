using System.Net.Http;
using System.Text.Json;
using Finance_Tracker_WPF_API.Core.Data;
using Finance_Tracker_WPF_API.Core.Models;
using Finance_Tracker_WPF_API.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Finance_Tracker_WPF_API.Core.Services;

public class ExchangeRateService : IExchangeRateService
{
    private readonly HttpClient _httpClient;
    private readonly ICurrencyRateRepository _currencyRateRepository;
    private readonly AppDbContext _context;

    public ExchangeRateService(AppDbContext context, ICurrencyRateRepository currencyRateRepository)
    {
        _httpClient = new HttpClient();
        _context = context;
        _currencyRateRepository = currencyRateRepository;
        Log.Debug("ExchangeRateService initialized");
    }

    public async Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency)
    {
        if (fromCurrency == toCurrency)
            return 1m;

        if (fromCurrency == "USD" || toCurrency == "USD")
        {
            var rate = await _context.CurrencyRates
                .OrderByDescending(r => r.Date)
                .FirstOrDefaultAsync(r => r.FromCurrency == fromCurrency && r.ToCurrency == toCurrency);
            if (rate != null)
                return rate.Rate;
            var reverse = await _context.CurrencyRates
                .OrderByDescending(r => r.Date)
                .FirstOrDefaultAsync(r => r.FromCurrency == toCurrency && r.ToCurrency == fromCurrency);
            if (reverse != null && reverse.Rate != 0)
                return 1m / reverse.Rate;
            throw new InvalidOperationException($"USD rate not found for {fromCurrency} to {toCurrency}");
        }

        var fromToUsd = await GetExchangeRateAsync(fromCurrency, "USD");
        var usdToTarget = await GetExchangeRateAsync("USD", toCurrency);
        return fromToUsd * usdToTarget;
    }

    public async Task UpdateExchangeRatesAsync()
    {
        try
        {
            Log.Debug("Updating exchange rates");

            var apiKey = AppConfig.ApiKey;
            if (string.IsNullOrEmpty(apiKey))
            {
                Log.Error("API key is not configured. Please set it in the application settings.");
                throw new InvalidOperationException("API key is not configured. Please set it in the application settings.");
            }

            var response = await _httpClient.GetStringAsync($"{AppConfig.Api.ExchangeRateBaseUrl}");
            var data = JsonDocument.Parse(response);

            var baseCurrency = "USD";
            var rates = data.RootElement.GetProperty("conversion_rates");

            foreach (var rate in rates.EnumerateObject())
            {
                var currencyRate = new CurrencyRate
                {
                    FromCurrency = baseCurrency,
                    ToCurrency = rate.Name,
                    Rate = rate.Value.GetDecimal(),
                    Date = DateTime.UtcNow
                };

                var existingRate = await _context.CurrencyRates
                    .FirstOrDefaultAsync(r => 
                        r.FromCurrency == currencyRate.FromCurrency && 
                        r.ToCurrency == currencyRate.ToCurrency);

                if (existingRate != null)
                {
                    existingRate.Rate = currencyRate.Rate;
                    existingRate.Date = currencyRate.Date;
                    await _currencyRateRepository.UpdateAsync(existingRate);
                }
                else
                {
                    await _currencyRateRepository.AddAsync(currencyRate);
                }
            }

            await _currencyRateRepository.SaveChangesAsync();
            Log.Information("Exchange rates updated successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating exchange rates");
            throw;
        }
    }

    public async Task<IEnumerable<CurrencyRate>> GetLatestRatesAsync(string baseCurrency)
    {
        try
        {
            Log.Debug("Getting latest exchange rates for {BaseCurrency}", baseCurrency);

            var rates = await _context.CurrencyRates
                .Where(r => r.FromCurrency == baseCurrency && r.Date >= DateTime.UtcNow.AddHours(-24))
                .ToListAsync();

            Log.Debug("Latest exchange rates: {Rates}", rates);
            return rates;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting latest exchange rates");
            throw;
        }
    }

    public Task<IEnumerable<string>> GetAvailableCurrenciesAsync()
    {
        var currencies = new List<string> { "USD", "EUR", "KGS", "RUB", "KZT", "GBP" };
        return Task.FromResult<IEnumerable<string>>(currencies);
    }
}