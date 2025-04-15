using System.Globalization;
using System.Windows.Data;
using Finance_Tracker_WPF_API.Core.Models;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace Finance_Tracker_WPF_API.UI.Converters;

public class BalanceLineChartConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not IEnumerable<Transaction> transactions)
            return Array.Empty<ISeries>();

        var dailyBalances = transactions
            .GroupBy(t => t.Date.Date)
            .OrderBy(g => g.Key)
            .Select(g => new
            {
                Date = g.Key,
                Balance = g.Sum(t => t.Type == TransactionType.Income ? t.Amount : -t.Amount)
            })
            .ToList();

        var cumulativeBalance = 0m;
        var balancePoints = dailyBalances
            .Select(d => cumulativeBalance += d.Balance)
            .ToArray();

        return new[]
        {
            new LineSeries<double>
            {
                Name = "Balance",
                Values = balancePoints.Select(b => (double)b).ToArray(),
                Fill = null,
                GeometrySize = 0,
                LineSmoothness = 0.5,
                Stroke = new SolidColorPaint(SKColors.DodgerBlue, 2)
            }
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
} 