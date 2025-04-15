using System.Collections.ObjectModel;
using System.Windows.Input;
using Finance_Tracker_WPF_API.Core.Models;
using Finance_Tracker_WPF_API.Core.Services;

namespace Finance_Tracker_WPF_API.UI.ViewModels;

public class TransactionDialogViewModel : ViewModelBase
{
    private readonly ITransactionService _transactionService;
    private decimal _amount;
    private string _description = string.Empty;
    private int _selectedCategoryId;
    private TransactionType _type;
    private string? _note;
    private ObservableCollection<Category> _categories;

    public TransactionDialogViewModel(ITransactionService transactionService)
    {
        _transactionService = transactionService;
        _categories = new ObservableCollection<Category>();

        SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => CanSave());
        CancelCommand = new RelayCommand(_ => CloseDialog());

        _ = LoadCategoriesAsync();
    }

    public decimal Amount
    {
        get => _amount;
        set => SetProperty(ref _amount, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public int SelectedCategoryId
    {
        get => _selectedCategoryId;
        set => SetProperty(ref _selectedCategoryId, value);
    }

    public TransactionType Type
    {
        get => _type;
        set => SetProperty(ref _type, value);
    }

    public string? Note
    {
        get => _note;
        set => SetProperty(ref _note, value);
    }

    public ObservableCollection<Category> Categories
    {
        get => _categories;
        set => SetProperty(ref _categories, value);
    }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    private async Task LoadCategoriesAsync()
    {
        // TODO: Implement category loading
    }

    private bool CanSave()
    {
        return Amount > 0 && !string.IsNullOrWhiteSpace(Description) && SelectedCategoryId > 0;
    }

    private async Task SaveAsync()
    {
        await _transactionService.CreateTransactionAsync(Amount, Description, SelectedCategoryId, Type, Note);
        CloseDialog();
    }

    private void CloseDialog()
    {
        // TODO: Implement dialog closing
    }
} 