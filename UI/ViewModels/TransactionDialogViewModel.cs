using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Finance_Tracker_WPF_API.Core.Models;

namespace Finance_Tracker_WPF_API.UI.ViewModels
{
    public class TransactionDialogViewModel : ViewModelBase
    {
        private decimal _amount;
        private string _description;
        private Category _selectedCategory;
        private ObservableCollection<Category> _categories;
        private ICommand _saveCommand;
        private ICommand _cancelCommand;

        public decimal Amount
        {
            get => _amount;
            set
            {
                _amount = value;
                OnPropertyChanged(nameof(Amount));
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged(nameof(SelectedCategory));
            }
        }

        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set
            {
                _categories = value;
                OnPropertyChanged(nameof(Categories));
            }
        }

        public ICommand SaveCommand
        {
            get => _saveCommand;
            set
            {
                _saveCommand = value;
                OnPropertyChanged(nameof(SaveCommand));
            }
        }

        public ICommand CancelCommand
        {
            get => _cancelCommand;
            set
            {
                _cancelCommand = value;
                OnPropertyChanged(nameof(CancelCommand));
            }
        }

        public TransactionDialogViewModel()
        {
            Categories = new ObservableCollection<Category>();
            SaveCommand = new RelayCommand(ExecuteSave);
            CancelCommand = new RelayCommand(ExecuteCancel);
        }

        private void ExecuteSave(object parameter)
        {
            if (Amount <= 0)
            {
                // Show error message
                return;
            }

            if (string.IsNullOrWhiteSpace(Description))
            {
                // Show error message
                return;
            }

            if (SelectedCategory == null)
            {
                // Show error message
                return;
            }

            // Create and return the transaction
            var transaction = new Transaction
            {
                Amount = Amount,
                Description = Description,
                Category = SelectedCategory,
                Date = DateTime.Now
            };

            // Close the dialog with result
            if (parameter is Window window)
            {
                window.DialogResult = true;
                window.Close();
            }
        }

        private void ExecuteCancel(object parameter)
        {
            if (parameter is Window window)
            {
                window.DialogResult = false;
                window.Close();
            }
        }
    }
} 