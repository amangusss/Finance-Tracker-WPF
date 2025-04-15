namespace Finance_Tracker_WPF_API.Core;

public static class AppConfig
{
    public static class Api
    {
        public const string ExchangeRateBaseUrl = "https://v6.exchangerate-api.com/v6/a1157b93e455afc5609b7429/latest/USD";
        public const string ApiKey = "a1157b93e455afc5609b7429"; // TODO: Move to secure storage
    }

    public static class Database
    {
        public const string ConnectionString = "Data Source=FinanceTracker.db";
    }

    public static class Export
    {
        public const string DefaultExportPath = "Exports";
        public const string CsvExtension = ".csv";
        public const string ExcelExtension = ".xlsx";
    }

    public static class Categories
    {
        public static readonly string[] DefaultExpenseCategories = 
        {
            "Продукты",
            "Транспорт",
            "Развлечения",
            "Коммунальные услуги",
            "Здоровье",
            "Одежда",
            "Другое"
        };

        public static readonly string[] DefaultIncomeCategories =
        {
            "Зарплата",
            "Фриланс",
            "Инвестиции",
            "Подарки",
            "Другое"
        };
    }
} 