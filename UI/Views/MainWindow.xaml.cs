using System.Windows;
using Finance_Tracker_WPF_API.UI.ViewModels;

namespace Finance_Tracker_WPF_API.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}