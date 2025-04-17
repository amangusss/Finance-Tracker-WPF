using System.Windows;
using Finance_Tracker_WPF_API.UI.ViewModels;
using Finance_Tracker_WPF_API.Core.Services;

namespace Finance_Tracker_WPF_API.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}