using System.Net.Http;
using System.Text.Json;
using Finance_Tracker_WPF_API.Core;
using Finance_Tracker_WPF_API.Core.Models;
using Finance_Tracker_WPF_API.Core.Repositories;
using Finance_Tracker_WPF_API.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Finance_Tracker_WPF_API.Core.Services;

public class ExchangeRateService : IExchangeRateService
{
    private readonly HttpClient _httpClient;
    private readonly IRepository<CurrencyRate> _currencyRateRepository;
    private readonly ApplicationDbContext _context;

    public ExchangeRateService(ApplicationDbContext context)
    {
        _httpClient = new HttpClient();
        _context = context;
        _currencyRateRepository = new Repository<CurrencyRate>(context);
    }

    public async Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency)
    {
        var rate = await _context.CurrencyRates
            .FirstOrDefaultAsync(r => 
                r.BaseCurrency == fromCurrency && 
                r.TargetCurrency == toCurrency &&
                r.LastUpdated >= DateTime.Now.AddHours(-24));

        if (rate == null)
        {
            await UpdateExchangeRatesAsync();
            rate = await _context.CurrencyRates
                .FirstOrDefaultAsync(r => 
                    r.BaseCurrency == fromCurrency && 
                    r.TargetCurrency == toCurrency);
        }

        return rate?.Rate ?? 1m;
    }

    public async Task UpdateExchangeRatesAsync()
    {
        var apiKey = AppConfig.ApiKey;
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("API key is not configured. Please set it in the application settings.");
        }

        var response = await _httpClient.GetStringAsync($"{AppConfig.Api.ExchangeRateBaseUrl}");
        var data = JsonDocument.Parse(response);

        var baseCurrency = data.RootElement.GetProperty("base").GetString();
        var rates = data.RootElement.GetProperty("rates");

        foreach (var rate in rates.EnumerateObject())
        {
            var currencyRate = new CurrencyRate
            {
                BaseCurrency = baseCurrency!,
                TargetCurrency = rate.Name,
                Rate = rate.Value.GetDecimal(),
                LastUpdated = DateTime.Now
            };

            var existingRate = await _context.CurrencyRates
                .FirstOrDefaultAsync(r => 
                    r.BaseCurrency == currencyRate.BaseCurrency && 
                    r.TargetCurrency == currencyRate.TargetCurrency);

            if (existingRate != null)
            {
                existingRate.Rate = currencyRate.Rate;
                existingRate.LastUpdated = currencyRate.LastUpdated;
                await _currencyRateRepository.UpdateAsync(existingRate);
            }
            else
            {
                await _currencyRateRepository.AddAsync(currencyRate);
            }
        }

        await _currencyRateRepository.SaveChangesAsync();
    }

    public async Task<IEnumerable<CurrencyRate>> GetLatestRatesAsync(string baseCurrency)
    {
        return await _context.CurrencyRates
            .Where(r => r.BaseCurrency == baseCurrency && r.LastUpdated >= DateTime.Now.AddHours(-24))
            .ToListAsync();
    }
} 