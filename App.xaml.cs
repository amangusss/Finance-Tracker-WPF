using System.IO;
using System.Windows;
using Finance_Tracker_WPF_API.Core;
using Finance_Tracker_WPF_API.Core.Data;
using Finance_Tracker_WPF_API.Core.Models;
using Finance_Tracker_WPF_API.Core.Patterns;
using Finance_Tracker_WPF_API.Core.Repositories;
using Finance_Tracker_WPF_API.Core.Services;
using Finance_Tracker_WPF_API.UI.ViewModels;
using Finance_Tracker_WPF_API.UI.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace Finance_Tracker_WPF_API;

public partial class App
{
    private ServiceProvider _serviceProvider;
    private const string DbPath = "FinanceTracker.db";
    private const string LogPath = "logs/finance-tracker.log";

    public App()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        Directory.CreateDirectory("logs");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .WriteTo.File(LogPath,
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog(Log.Logger, dispose: true);
        });

        var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DbPath);
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlite($"Data Source={dbPath}");
            options.EnableSensitiveDataLogging();
            options.LogTo(message => Log.Debug(message));
        });

        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICurrencyRateRepository, CurrencyRateRepository>();

        services.AddScoped<ITransactionFactory, TransactionFactory>();

        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IExchangeRateService, ExchangeRateService>();

        services.AddTransient<MainViewModel>();
        services.AddTransient<TransactionDialogViewModel>();

        services.AddTransient<MainWindow>();
        services.AddTransient<TransactionDialog>(provider =>
        {
            var viewModel = provider.GetRequiredService<TransactionDialogViewModel>();
            var dialog = new TransactionDialog(viewModel);
            dialog.Owner = Current?.MainWindow;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            return dialog;
        });
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            Log.Information("Application starting up");

            AppConfig.ApiKey = "a1157b93e455afc5609b7429";

            var dbContext = _serviceProvider.GetRequiredService<AppDbContext>();
            Log.Information("Creating database if not exists at: {DbPath}", DbPath);
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();

            var categoryRepository = _serviceProvider.GetRequiredService<ICategoryRepository>();
            foreach (var categoryName in AppConfig.Categories.DefaultExpenseCategories)
            {
                await categoryRepository.AddAsync(new Category { Name = categoryName, Type = TransactionType.Expense });
            }
            foreach (var categoryName in AppConfig.Categories.DefaultIncomeCategories)
            {
                await categoryRepository.AddAsync(new Category { Name = categoryName, Type = TransactionType.Income });
            }
            await categoryRepository.SaveChangesAsync();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            Log.Information("Application startup completed successfully");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application failed to start");
            MessageBox.Show($"Application failed to start: {ex.Message}", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Current.Shutdown();
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("Application shutting down");
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}