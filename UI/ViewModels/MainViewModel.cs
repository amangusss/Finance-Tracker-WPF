using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Finance_Tracker_WPF_API.Core.Models;
using Finance_Tracker_WPF_API.Core.Services;
using Finance_Tracker_WPF_API.UI.Views;
using Serilog;

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
    private ObservableCollection<TransactionDisplay> _displayTransactions = new();

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
        _startDate = DateTime.Today.AddMonths(-1);
        _endDate = DateTime.Today;
        SelectedCurrency = "USD";

        LoadDataCommand = new RelayCommand(async _ => await LoadDataAsync());
        AddTransactionCommand = new RelayCommand(async _ => await ShowAddTransactionDialogAsync());
        DeleteTransactionCommand = new RelayCommand(async param => await DeleteTransactionAsync(param as Transaction));
        UpdateExchangeRateCommand = new RelayCommand(async _ => await UpdateExchangeRateAsync());

        _ = LoadDataAsync();
        _ = UpdateExchangeRateAsync();

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

    public ObservableCollection<TransactionDisplay> DisplayTransactions
    {
        get => _displayTransactions;
        set => SetProperty(ref _displayTransactions, value);
    }

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

    public async Task<List<TransactionDisplay>> GetDisplayTransactionsAsync(IEnumerable<Transaction> transactions)
    {
        var list = new List<TransactionDisplay>();
        foreach (var t in transactions)
        {
            var amountConv = await ConvertToSelectedCurrencyAsync(t);
            list.Add(new TransactionDisplay
            {
                Id = t.Id,
                Amount = t.Amount,
                AmountInSelectedCurrency = amountConv,
                Currency = t.Currency,
                Date = t.Date,
                Description = t.Description,
                Type = t.Type,
                Category = t.Category,
                CategoryId = t.CategoryId,
                Note = t.Note
            });
        }
        return list;
    }

    public class TransactionDisplay : Transaction
    {
        public decimal AmountInSelectedCurrency { get; set; }
    }

    public async Task UpdateAllAmountsAndChartsAsync()
    {
        _currencyRateCache.Clear();
        var displayTx = await GetDisplayTransactionsAsync(Transactions);
        TotalIncome = displayTx.Where(t => t.Type == TransactionType.Income).Sum(t => t.AmountInSelectedCurrency);
        TotalExpense = displayTx.Where(t => t.Type == TransactionType.Expense).Sum(t => t.AmountInSelectedCurrency);
        Balance = TotalIncome - TotalExpense;
        DisplayTransactions = new ObservableCollection<TransactionDisplay>(displayTx);
        OnPropertyChanged(nameof(DisplayTransactions));
    }

    public async Task LoadDataAsync()
    {
        try
        {
            Log.Debug("Loading transactions");
            var transactions = await _transactionService.GetTransactionsAsync(_startDate, _endDate);
            Transactions = new ObservableCollection<Transaction>(transactions);
            var currencies = await _exchangeRateService.GetAvailableCurrenciesAsync();
            AvailableCurrencies = new ObservableCollection<string>(currencies);
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

    private void UpdateChartData()
    {
        try
        {
            CategoryTotals.Clear();
            var categoryGroups = Transactions
                .Where(t => t.Category != null)
                .GroupBy(t => t.Category)
                .Select(g => new Transaction
                {
                    Category = g.Key,
                    Amount = g.Sum(t => t.Type == TransactionType.Income ? t.Amount : -t.Amount),
                    Type = g.Sum(t => t.Type == TransactionType.Income ? t.Amount : -t.Amount) > 0 
                        ? TransactionType.Income 
                        : TransactionType.Expense,
                    Date = DateTime.Now,
                    Description = g.Key?.Name ?? "Unknown"
                })
                .ToList();

            foreach (var categoryTotal in categoryGroups)
            {
                CategoryTotals.Add(categoryTotal);
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

            Log.Debug("Chart data updated. Categories: {Count}", CategoryTotals.Count);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating chart data: {Message}", ex.Message);
        }
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
                    await LoadDataAsync();
                }
            };
            var dialog = new TransactionDialog(dialogViewModel);
            Log.Debug("Opening transaction dialog");
            if (dialog.ShowDialog() == true)
            {
                await LoadDataAsync();
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