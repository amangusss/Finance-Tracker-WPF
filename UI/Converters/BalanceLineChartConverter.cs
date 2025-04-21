using System.Globalization;
using System.Windows.Data;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Finance_Tracker_WPF_API.Core.Models;

namespace Finance_Tracker_WPF_API.UI.Converters;

public class BalanceLineChartConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IEnumerable<Transaction> transactions && transactions.Any())
        {
            var sortedTransactions = transactions.OrderBy(t => t.Date).ToList();
            var runningBalance = 0m;
            
            var points = new List<decimal>();
            foreach (var transaction in sortedTransactions)
            {
                runningBalance += transaction.Type == TransactionType.Income ? transaction.Amount : -transaction.Amount;
                points.Add(runningBalance);
            }

            if (points.Any())
            {
                return new ISeries[]
                {
                    new LineSeries<decimal>
                    {
                        Values = points.ToArray(),
                        Name = "Balance",
                        Fill = null,
                        GeometrySize = 10,
                        Stroke = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint
                        {
                            Color = new SkiaSharp.SKColor(25, 118, 210),
                            StrokeThickness = 3
                        }
                    }
                };
            }
        }

        return new ISeries[]
        {
            new LineSeries<decimal>
            {
                Values = new decimal[] { 0 },
                Name = "No Data",
                Fill = null
            }
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
} 