using Finance_Tracker_WPF_API.Core.Configuration;

namespace Finance_Tracker_WPF_API.Core;

public static class AppConfig
{
    private static readonly AppSettings Settings = AppSettings.Load();

    public static string ApiKey
    {
        get => Settings.ApiKey;
        set
        {
            Settings.ApiKey = value;
            Settings.Save();
        }
    }

    public static class Api
    {
        public static string ExchangeRateBaseUrl => $"https://v6.exchangerate-api.com/v6/{ApiKey}/latest/USD";
    }

    public static class Database
    {
        public static string ConnectionString => "Data Source=FinanceTracker.db";
    }

    public static class Categories
    {
        public static string[] DefaultExpenseCategories => new[]
        {
            "Food",
            "Transport",
            "Entertainment",
            "Shopping",
            "Bills",
            "Health",
            "Education",
            "Other"
        };

        public static string[] DefaultIncomeCategories => new[]
        {
            "Salary",
            "Freelance",
            "Investments",
            "Gifts",
            "Other"
        };
    }

    public static class Export
    {
        public static string CsvExtension => ".csv";
    }
}