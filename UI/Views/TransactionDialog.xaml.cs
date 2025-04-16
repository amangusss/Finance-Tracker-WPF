using System.Windows;
using Finance_Tracker_WPF_API.UI.ViewModels;

namespace Finance_Tracker_WPF_API.UI.Views
{
    public partial class TransactionDialog : Window
    {
        private TransactionDialogViewModel ViewModel => (TransactionDialogViewModel)DataContext;

        public TransactionDialog(TransactionDialogViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка валидности данных
            if (ViewModel.Amount <= 0)
            {
                MessageBox.Show("Amount must be greater than 0", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(ViewModel.Description))
            {
                MessageBox.Show("Please enter a description", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (ViewModel.SelectedCategory == null)
            {
                MessageBox.Show("Please select a category", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Сохранение транзакции
            bool success = await ViewModel.SaveTransaction();
            if (success)
            {
                DialogResult = true;
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}