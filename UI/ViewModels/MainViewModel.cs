using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Finance_Tracker_WPF_API.Core.Models;
using Finance_Tracker_WPF_API.Core.Services;
using Finance_Tracker_WPF_API.UI.Views;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using Microsoft.Extensions.DependencyInjection;
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

    public MainViewModel(IServiceProvider serviceProvider, ITransactionService transactionService, IExchangeRateService exchangeRateService)
    {
        _serviceProvider = serviceProvider;
        _transactionService = transactionService;
        _exchangeRateService = exchangeRateService;
        _transactions = new ObservableCollection<Transaction>();
        _categoryTotals = new ObservableCollection<Transaction>();
        _availableCurrencies = new ObservableCollection<string> { "USD", "EUR", "RUB", "KZT" };
        _startDate = DateTime.Today.AddMonths(-1);
        _endDate = DateTime.Today;

        LoadDataCommand = new RelayCommand(async _ => await LoadDataAsync());
        AddTransactionCommand = new RelayCommand(async _ => await ShowAddTransactionDialogAsync());
        DeleteTransactionCommand = new RelayCommand(async param => await DeleteTransactionAsync(param as Transaction));
        UpdateExchangeRateCommand = new RelayCommand(async _ => await UpdateExchangeRateAsync());

        // Загружаем данные при инициализации
        _ = LoadDataAsync();
        _ = UpdateExchangeRateAsync();

        Log.Information("MainViewModel initialized");
    }

    public ObservableCollection<string> AvailableCurrencies
    {
        get => _availableCurrencies;
        set => SetProperty(ref _availableCurrencies, value);
    }

    public ObservableCollection<Transaction> Transactions
    {
        get => _transactions;
        set => SetProperty(ref _transactions, value);
    }

    public ObservableCollection<Transaction> CategoryTotals
    {
        get => _categoryTotals;
        set => SetProperty(ref _categoryTotals, value);
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

    public DateTime StartDate
    {
        get => _startDate;
        set
        {
            if (SetProperty(ref _startDate, value))
            {
                Log.Debug("Start date changed to: {StartDate}", value);
                _ = LoadDataAsync();
            }
        }
    }

    public DateTime EndDate
    {
        get => _endDate;
        set
        {
            if (SetProperty(ref _endDate, value))
            {
                Log.Debug("End date changed to: {EndDate}", value);
                _ = LoadDataAsync();
            }
        }
    }

    public string SelectedCurrency
    {
        get => _selectedCurrency;
        set
        {
            if (SetProperty(ref _selectedCurrency, value))
            {
                Log.Debug("Selected currency changed to: {Currency}", value);
                _ = UpdateExchangeRateAsync();
            }
        }
    }

    public decimal ExchangeRate
    {
        get => _exchangeRate;
        set => SetProperty(ref _exchangeRate, value);
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

    public ICommand LoadDataCommand { get; }
    public ICommand AddTransactionCommand { get; }
    public ICommand DeleteTransactionCommand { get; }
    public ICommand UpdateExchangeRateCommand { get; }

    public async Task LoadDataAsync()
    {
        try
        {
            Log.Debug("Loading transactions");
            var transactions = await _transactionService.GetTransactionsAsync();

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Transactions.Clear();
                CategoryTotals.Clear();

                foreach (var transaction in transactions)
                {
                    Transactions.Add(transaction);
                }

                UpdateTotalAmounts();
                UpdateChartData();
                Log.Information("Transactions loaded successfully. Count: {Count}", Transactions.Count);
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error loading transactions");
            MessageBox.Show($"Error loading transactions: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void UpdateTotalAmounts()
    {
        TotalIncome = Transactions
            .Where(t => t.Type == TransactionType.Income)
            .Sum(t => t.Amount);

        TotalExpense = Transactions
            .Where(t => t.Type == TransactionType.Expense)
            .Sum(t => t.Amount);

        Balance = TotalIncome - TotalExpense;

        Log.Debug("Total amounts updated: Income={Income}, Expense={Expense}, Balance={Balance}",
            TotalIncome, TotalExpense, Balance);
    }

    private void UpdateChartData()
    {
        try
        {
            // Обновляем данные для круговой диаграммы категорий
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

            // Формируем подписи для оси X (месяц.год)
            var grouped = Transactions
                .GroupBy(t => new { t.Date.Year, t.Date.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .ToList();
            IncomeExpenseChartLabels = grouped.Select(g => $"{g.Key.Month:D2}.{g.Key.Year}").ToList();

            // Гарантируем, что IncomeExpenseAxes совпадает по длине с Series
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
            var dialogViewModel = _serviceProvider.GetRequiredService<TransactionDialogViewModel>();
            dialogViewModel.TransactionCompleted += async (sender, success) =>
            {
                if (success)
                {
                    await LoadDataAsync();
                }
            };

            var dialog = _serviceProvider.GetRequiredService<TransactionDialog>();
            
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
            Log.Information("Transaction deleted successfully");
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
}