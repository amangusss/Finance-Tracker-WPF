using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Finance_Tracker_WPF_API.Core.Models;
using Finance_Tracker_WPF_API.Core.Services;
using Serilog;

namespace Finance_Tracker_WPF_API.UI.ViewModels;

public class TransactionDialogViewModel : ViewModelBase
{
    private readonly ITransactionService _transactionService;
    private string _amountText = string.Empty;
    private decimal _amount;
    private string _description = string.Empty;
    private Category? _selectedCategory;
    private ObservableCollection<Category> _categories = new();
    private TransactionType _transactionType = TransactionType.Expense; 
    private string? _note;
    private string _currency = "USD";
    private readonly ICommand _saveCommand;
    private readonly ICommand _cancelCommand;

    public event EventHandler<bool>? TransactionCompleted;

    public TransactionDialogViewModel(ITransactionService transactionService, string selectedCurrency)
    {
        _transactionService = transactionService;
        Currency = selectedCurrency;
        _saveCommand = new RelayCommand(ExecuteSave, _ => true);
        _cancelCommand = new RelayCommand(ExecuteCancel, _ => true);

        LoadCategories();
    }

    public string AmountText
    {
        get => _amountText;
        set
        {
            if (SetProperty(ref _amountText, value))
            {
                Log.Debug("Amount text changed to: {Amount}", value);
                if (decimal.TryParse(value?.Replace(",", "."), out decimal parsedAmount))
                {
                    Amount = parsedAmount;
                }
            }
        }
    }

    public decimal Amount
    {
        get => _amount;
        private set
        {
            if (SetProperty(ref _amount, value))
            {
                Log.Debug("Amount changed to: {Amount}", value);
            }
        }
    }

    public string Description
    {
        get => _description;
        set
        {
            if (SetProperty(ref _description, value))
            {
                Log.Debug("Description changed to: {Description}", value);
            }
        }
    }

    public Category? SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (SetProperty(ref _selectedCategory, value))
            {
                Log.Debug("Selected category changed to: {Category}", value?.Name);
            }
        }
    }

    public ObservableCollection<Category> Categories
    {
        get => _categories;
        set => SetProperty(ref _categories, value);
    }

    public string Currency
    {
        get => _currency;
        set => SetProperty(ref _currency, value);
    }

    public ICommand SaveCommand => _saveCommand;
    public ICommand CancelCommand => _cancelCommand;

    public TransactionType TransactionType
    {
        get => _transactionType;
        set
        {
            if (SetProperty(ref _transactionType, value))
            {
                Log.Debug("Transaction type changed to: {TransactionType}", value);
                LoadCategories();
            }
        }
    }

    public IEnumerable<TransactionType> TransactionTypes => Enum.GetValues(typeof(TransactionType)).Cast<TransactionType>();

    private async void ExecuteSave(object? parameter)
    {
        try
        {
            Log.Debug("Attempting to save transaction");

            if (Amount <= 0)
            {
                MessageBox.Show("Amount must be greater than 0", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Log.Warning("Amount must be greater than 0: {Amount}", Amount);
                return;
            }

            if (string.IsNullOrWhiteSpace(_description))
            {
                MessageBox.Show("Please enter a description", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Log.Warning("Empty description");
                return;
            }

            if (_selectedCategory == null)
            {
                MessageBox.Show("Please select a category", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Log.Warning("No category selected");
                return;
            }

            await SaveTransaction();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error saving transaction");
            MessageBox.Show($"Error saving transaction: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            TransactionCompleted?.Invoke(this, false);
        }
    }

    public async Task<bool> SaveTransaction()
    {
        try
        {
            Log.Debug("Saving transaction: Type={TransactionType}, Amount={Amount}, Description={Description}, Category={Category}",
                _transactionType, Amount, _description, _selectedCategory?.Name);

            if (_selectedCategory == null)
            {
                return false;
            }

            var transaction = await _transactionService.CreateTransactionAsync(Amount, _description, _selectedCategory.Id, _transactionType, _note, Currency);
            
            Log.Information("Transaction created successfully: {TransactionId}", transaction.Id);
            TransactionCompleted?.Invoke(this, true);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error saving transaction");
            MessageBox.Show($"Error saving transaction: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            TransactionCompleted?.Invoke(this, false);
            return false;
        }
    }

    private void ExecuteCancel(object? parameter)
    {
        Log.Debug("Transaction dialog cancelled");
    }

    private async void LoadCategories()
    {
        try
        {
            Log.Debug("Loading categories for transaction type: {TransactionType}", _transactionType);
            var categories = await _transactionService.GetCategoriesByTypeAsync(_transactionType);

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var uniqueCategories = categories
                    .GroupBy(c => c.Name)
                    .Select(g => g.First())
                    .ToList();
                Categories = new ObservableCollection<Category>(uniqueCategories);

                if (Categories.Any())
                {
                    SelectedCategory = Categories.First();
                }
                else
                {
                    Log.Warning("No categories found for transaction type: {TransactionType}", _transactionType);
                    MessageBox.Show($"No categories found for {_transactionType}. Please add categories first.",
                        "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error loading categories");
            MessageBox.Show($"Error loading categories: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}