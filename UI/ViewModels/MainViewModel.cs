using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Finance_Tracker_WPF_API.Core.Models;
using Finance_Tracker_WPF_API.Core.Services;
using Finance_Tracker_WPF_API.UI.Views;
using Serilog;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Linq;
using LiveChartsCore.SkiaSharpView;

namespace Finance_Tracker_WPF_API.UI.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly ITransactionService _transactionService;
    private readonly IExchangeRateService _exchangeRateService;
    private readonly IServiceProvider _serviceProvider;
    private ObservableCollection<Transaction> _transactions;
    private ObservableCollection<Transaction> _categoryTotals;
    private decimal _balance;
    private decimal _totalIncome;
    private decimal _totalExpense;
    private DateTime _startDate;
    private DateTime _endDate;
    private string _selectedCurrency = "USD";
    private decimal _exchangeRate = 1m;
    private ObservableCollection<string> _availableCurrencies;
    private List<string> _incomeExpenseChartLabels = new();
    private IEnumerable<LiveChartsCore.SkiaSharpView.Axis> _incomeExpenseAxes;
    private IEnumerable<LiveChartsCore.SkiaSharpView.Axis> _dailyChartAxes = new List<LiveChartsCore.SkiaSharpView.Axis>();
    private ObservableCollection<TransactionDisplayDTO> _displayTransactions = new();
    public ObservableCollection<Category> AllCategories { get; set; } = new();

    public MainViewModel(IServiceProvider serviceProvider, ITransactionService transactionService, IExchangeRateService exchangeRateService)
    {
        _serviceProvider = serviceProvider;
        _transactionService = transactionService;
        _exchangeRateService = exchangeRateService;
        _transactions = new ObservableCollection<Transaction>();
        _transactions.CollectionChanged += (s, e) =>
        {
            UpdateTotalAmounts();
            UpdateChartData();
        };
        _categoryTotals = new ObservableCollection<Transaction>();
        _availableCurrencies = new ObservableCollection<string> { "USD", "EUR", "RUB", "KZT", "UAH", "KGS" };
        SelectedCurrency = "USD";
        _startDate = DateTime.Today.AddMonths(-1);
        _endDate = DateTime.Today;

        LoadDataCommand = new RelayCommand(async _ => await LoadDataAsync());
        AddTransactionCommand = new RelayCommand(async _ => await ShowAddTransactionDialogAsync());
        DeleteTransactionCommand = new RelayCommand(async param => await DeleteTransactionAsync(param as Transaction));
        UpdateExchangeRateCommand = new RelayCommand(async _ => await UpdateExchangeRateAsync());
        DeleteDisplayTransactionCommand = new RelayCommand(async param => await DeleteDisplayTransactionAsync(param as TransactionDisplayDTO));
        DeleteAllTransactionsCommand = new RelayCommand(async _ => await DeleteAllTransactionsAsync());

        _ = UpdateExchangeRateAsync();
        _ = LoadDataAsync().ContinueWith(async t => await UpdateAllAmountsAndChartsAsync());    
        _ = LoadAllCategoriesAsync();

        Log.Information("MainViewModel initialized");
    }

    public ObservableCollection<Transaction> Transactions
    {
        get => _transactions;
        set
        {
            if (SetProperty(ref _transactions, value))
            {
                if (_transactions != null)
                {
                    _transactions.CollectionChanged += (s, e) =>
                    {
                        UpdateTotalAmounts();
                        UpdateChartData();
                    };
                }
                UpdateTotalAmounts();
                UpdateChartData();
            }
        }
    }

    public ObservableCollection<Transaction> CategoryTotals
    {
        get => _categoryTotals;
    }

    public decimal Balance
    {
        get => _balance;
        set => SetProperty(ref _balance, value);
    }

    public decimal TotalIncome
    {
        get => _totalIncome;
        set => SetProperty(ref _totalIncome, value);
    }

    public decimal TotalExpense
    {
        get => _totalExpense;
        set => SetProperty(ref _totalExpense, value);
    }

    public string SelectedCurrency
    {
        get => _selectedCurrency;
        set
        {
            if (_selectedCurrency != value)
            {
                _selectedCurrency = value;
                OnPropertyChanged();
                try
                {
                    _ = UpdateExchangeRateAsync();
                    _ = UpdateAllAmountsAndChartsAsync();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error when changing the currency");
                }
            }
        }
    }

    public decimal ExchangeRate
    {
        get => _exchangeRate;
        set => SetProperty(ref _exchangeRate, value);
    }

    public ObservableCollection<string> AvailableCurrencies
    {
        get => _availableCurrencies;
        set => SetProperty(ref _availableCurrencies, value);
    }

    public List<string> IncomeExpenseChartLabels
    {
        get => _incomeExpenseChartLabels;
        set => SetProperty(ref _incomeExpenseChartLabels, value);
    }

    public IEnumerable<LiveChartsCore.SkiaSharpView.Axis> IncomeExpenseAxes
    {
        get => _incomeExpenseAxes;
        set => SetProperty(ref _incomeExpenseAxes, value);
    }

    public IEnumerable<LiveChartsCore.SkiaSharpView.Axis> DailyChartAxes
    {
        get => _dailyChartAxes;
        set => SetProperty(ref _dailyChartAxes, value);
    }

    public ICommand LoadDataCommand { get; }
    public ICommand AddTransactionCommand { get; }
    public ICommand DeleteTransactionCommand { get; }
    public ICommand UpdateExchangeRateCommand { get; }
    public ICommand DeleteDisplayTransactionCommand { get; }
    public ICommand DeleteAllTransactionsCommand { get; }

    public ObservableCollection<TransactionDisplayDTO> DisplayTransactions => _displayTransactions;

    private readonly Dictionary<string, decimal> _currencyRateCache = new();

    public async Task<decimal> ConvertToSelectedCurrencyAsync(Transaction t)
    {
        if (t.Currency == SelectedCurrency)
            return t.Amount;
        string key = t.Currency + "_" + SelectedCurrency;
        if (!_currencyRateCache.TryGetValue(key, out var rate))
        {
            rate = await _exchangeRateService.GetExchangeRateAsync(t.Currency, SelectedCurrency);
            _currencyRateCache[key] = rate;
        }
        return t.Amount * rate;
    }

    public async Task<List<TransactionDisplayDTO>> GetDisplayTransactionsAsync(IEnumerable<Transaction> transactions)
    {
        var list = new List<TransactionDisplayDTO>();
        foreach (var t in transactions)
        {
            var amountConv = await ConvertToSelectedCurrencyAsync(t);
            list.Add(new TransactionDisplayDTO
            {
                Id = t.Id,
                Amount = t.Amount,
                AmountInSelectedCurrency = amountConv,
                Currency = t.Currency ?? "",
                Date = t.Date,
                Description = t.Description ?? "",
                Type = t.Type.ToString(),
                CategoryName = t.Category?.Name ?? string.Empty,
                Note = t.Note ?? ""
            });
        }
        return list;
    }

    public class TransactionDisplayDTO
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountInSelectedCurrency { get; set; }
        public string Currency { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string CategoryName { get; set; }
        public string Note { get; set; }
    }

    public async Task UpdateAllAmountsAndChartsAsync()
    {
        _currencyRateCache.Clear();
        var displayTx = await GetDisplayTransactionsAsync(Transactions);
        TotalIncome = displayTx.Where(t => t.Type == TransactionType.Income.ToString()).Sum(t => t.AmountInSelectedCurrency);
        TotalExpense = displayTx.Where(t => t.Type == TransactionType.Expense.ToString()).Sum(t => t.AmountInSelectedCurrency);
        Balance = TotalIncome - TotalExpense;
        _displayTransactions.Clear();
        foreach (var tx in displayTx)
            _displayTransactions.Add(tx);
        OnPropertyChanged(nameof(DisplayTransactions));

        var expenseGroups = _displayTransactions
            .Where(tx => tx.Type == TransactionType.Expense.ToString())
            .GroupBy(tx => string.IsNullOrEmpty(tx.CategoryName) ? "Без категории" : tx.CategoryName)
            .Select(g => new { Category = g.Key, Total = g.Sum(tx => tx.AmountInSelectedCurrency) })
            .Where(x => x.Total > 0)
            .ToList();
        var pastelPalette = new[]
        {
            SKColor.Parse("#A7C7E7"), SKColor.Parse("#FFD6E0"), SKColor.Parse("#FFE5B4"), SKColor.Parse("#B5EAD7"),
            SKColor.Parse("#C7CEEA"), SKColor.Parse("#FFFACD"), SKColor.Parse("#FFDAC1"), SKColor.Parse("#E2F0CB"),
            SKColor.Parse("#B5B9FF"), SKColor.Parse("#E0BBE4"),
        };
        if (expenseGroups.Any())
        {
            var totalAll = expenseGroups.Sum(x => x.Total);
            ExpenseCategorySeries = expenseGroups.Select((x, i) =>
                (ISeries)new PieSeries<decimal>
                {
                    Values = new[] { x.Total },
                    Name = x.Category,
                    InnerRadius = 50,
                    DataLabelsSize = 14,
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Outer,
                    DataLabelsFormatter = point =>
                        $"{x.Category}: {(x.Total / totalAll).ToString("P2")} ({x.Total.ToString("F2")})",
                    Fill = new SolidColorPaint(pastelPalette[i % pastelPalette.Length])
                }).ToArray();
        }
        else
        {
            ExpenseCategorySeries = new ISeries[]
            {
                new PieSeries<decimal>
                {
                    Values = new decimal[] { 1 },
                    Name = "No Data",
                    InnerRadius = 50,
                    DataLabelsSize = 14,
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Outer,
                    DataLabelsFormatter = _ => "No Data",
                    Fill = new SolidColorPaint(SKColors.LightGray)
                }
            };
        }

        var grouped = Transactions
            .GroupBy(t => new { t.Date.Year, t.Date.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .ToList();
        IncomeExpenseChartLabels = grouped.Select(g => $"{g.Key.Month:D2}.{g.Key.Year}").ToList();

        IncomeExpenseAxes = new[]
        {
            new LiveChartsCore.SkiaSharpView.Axis
            {
                Labels = IncomeExpenseChartLabels,
                Name = "Month"
            }
        };
        OnPropertyChanged(nameof(IncomeExpenseAxes));

        Log.Debug("Chart data updated. Categories: {Count}", expenseGroups.Count);
    }

    public async Task LoadDataAsync()
    {
        try
        {
            Log.Debug("Loading transactions");
            var transactions = await _transactionService.GetTransactionsAsync(_startDate, _endDate);
            _transactions.Clear();
            foreach (var tx in transactions)
                _transactions.Add(tx);
            var currencies = await _exchangeRateService.GetAvailableCurrenciesAsync();
            _availableCurrencies.Clear();
            foreach (var c in currencies)
                _availableCurrencies.Add(c);
            if (string.IsNullOrWhiteSpace(SelectedCurrency))
                SelectedCurrency = "USD";
            await UpdateAllAmountsAndChartsAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error loading data: {Message}", ex.Message);
        }
    }

    public void UpdateTotalAmounts()
    {
        TotalIncome = Transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        TotalExpense = Transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        Balance = TotalIncome - TotalExpense;
    }

    private ISeries[] _expenseCategorySeries = Array.Empty<ISeries>();
    public ISeries[] ExpenseCategorySeries
    {
        get => _expenseCategorySeries;
        set => SetProperty(ref _expenseCategorySeries, value);
    }

    private async void UpdateChartData()
    {
        await UpdateAllAmountsAndChartsAsync();
        return;
    }

    public async Task LoadAllCategoriesAsync()
    {
        var categories = await _transactionService.GetAllCategoriesAsync();
        AllCategories.Clear();
        foreach (var cat in categories)
            AllCategories.Add(cat);
        OnPropertyChanged(nameof(AllCategories));
    }

    private async Task ShowAddTransactionDialogAsync()
    {
        try
        {
            var dialogViewModel = new TransactionDialogViewModel(_transactionService, SelectedCurrency);
            dialogViewModel.TransactionCompleted += async (sender, success) =>
            {
                if (success)
                {
                    var prevCurrency = SelectedCurrency;
                    await LoadDataAsync();
                    SelectedCurrency = prevCurrency;
                }
            };
            var dialog = new TransactionDialog(dialogViewModel);
            Log.Debug("Opening transaction dialog");
            if (dialog.ShowDialog() == true)
            {
                var prevCurrency = SelectedCurrency;
                await LoadDataAsync();
                SelectedCurrency = prevCurrency;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error showing add transaction dialog");
            MessageBox.Show($"Error showing dialog: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task DeleteTransactionAsync(Transaction? transaction)
    {
        if (transaction == null) return;

        try
        {
            Log.Debug("Deleting transaction: {TransactionId}", transaction.Id);
            await _transactionService.DeleteTransactionAsync(transaction.Id);
            await LoadDataAsync();
            await UpdateAllAmountsAndChartsAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error deleting transaction");
            MessageBox.Show($"Error deleting transaction: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task DeleteDisplayTransactionAsync(TransactionDisplayDTO dto)
    {
        if (dto == null) return;
        try
        {
            await _transactionService.DeleteTransactionAsync(dto.Id);
            await LoadDataAsync();
            await UpdateAllAmountsAndChartsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error deleting transaction: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task DeleteAllTransactionsAsync()
    {
        var list = _displayTransactions.ToList();
        try
        {
            foreach (var dto in list)
            {
                await _transactionService.DeleteTransactionAsync(dto.Id);
            }
            await LoadDataAsync();
            await UpdateAllAmountsAndChartsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error deleting all transactions: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task UpdateExchangeRateAsync()
    {
        try
        {
            Log.Debug("Updating exchange rate for {Currency}", SelectedCurrency);
            await _exchangeRateService.UpdateExchangeRatesAsync();
            ExchangeRate = await _exchangeRateService.GetExchangeRateAsync("USD", SelectedCurrency);
            Log.Debug("Exchange rate updated to: {Rate}", ExchangeRate);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating exchange rate");
            MessageBox.Show($"Error updating exchange rate: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void UpdateCurrencyAsync()
    {
        await UpdateExchangeRateAsync();
        await UpdateAllAmountsAndChartsAsync();
    }
}