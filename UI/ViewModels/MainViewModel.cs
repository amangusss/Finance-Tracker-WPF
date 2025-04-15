using System.Collections.ObjectModel;
using System.Windows.Input;
using Finance_Tracker_WPF_API.Core.Models;
using Finance_Tracker_WPF_API.Core.Patterns;
using Finance_Tracker_WPF_API.Core.Services;

namespace Finance_Tracker_WPF_API.UI.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly ITransactionService _transactionService;
    private readonly IExchangeRateService _exchangeRateService;
    private ObservableCollection<Transaction> _transactions;
    private decimal _balance;
    private DateTime _startDate;
    private DateTime _endDate;
    private string _selectedCurrency = "USD";
    private decimal _exchangeRate = 1m;

    public MainViewModel(
        ITransactionService transactionService,
        IExchangeRateService exchangeRateService)
    {
        _transactionService = transactionService;
        _exchangeRateService = exchangeRateService;
        _transactions = new ObservableCollection<Transaction>();
        _startDate = DateTime.Today.AddMonths(-1);
        _endDate = DateTime.Today;

        LoadDataCommand = new RelayCommand(async _ => await LoadDataAsync());
        AddTransactionCommand = new RelayCommand(async _ => await AddTransactionAsync());
        DeleteTransactionCommand = new RelayCommand(async param => await DeleteTransactionAsync(param as Transaction));
        UpdateExchangeRateCommand = new RelayCommand(async _ => await UpdateExchangeRateAsync());

        _ = LoadDataAsync();
    }

    public ObservableCollection<Transaction> Transactions
    {
        get => _transactions;
        set => SetProperty(ref _transactions, value);
    }

    public decimal Balance
    {
        get => _balance;
        set => SetProperty(ref _balance, value);
    }

    public DateTime StartDate
    {
        get => _startDate;
        set
        {
            if (SetProperty(ref _startDate, value))
            {
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
                _ = UpdateExchangeRateAsync();
            }
        }
    }

    public decimal ExchangeRate
    {
        get => _exchangeRate;
        set => SetProperty(ref _exchangeRate, value);
    }

    public ICommand LoadDataCommand { get; }
    public ICommand AddTransactionCommand { get; }
    public ICommand DeleteTransactionCommand { get; }
    public ICommand UpdateExchangeRateCommand { get; }

    private async Task LoadDataAsync()
    {
        var transactions = await _transactionService.GetTransactionsAsync(StartDate, EndDate);
        Transactions.Clear();
        foreach (var transaction in transactions)
        {
            Transactions.Add(transaction);
        }

        Balance = await _transactionService.GetBalanceAsync(StartDate, EndDate);
    }

    private async Task AddTransactionAsync()
    {
        // TODO: Implement transaction dialog
    }

    private async Task DeleteTransactionAsync(Transaction? transaction)
    {
        if (transaction == null) return;

        await _transactionService.DeleteTransactionAsync(transaction.Id);
        await LoadDataAsync();
    }

    private async Task UpdateExchangeRateAsync()
    {
        await _exchangeRateService.UpdateExchangeRatesAsync();
        ExchangeRate = await _exchangeRateService.GetExchangeRateAsync("USD", SelectedCurrency);
    }
} 