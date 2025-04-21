using System.Globalization;
using System.Windows.Data;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using SkiaSharp;
using LiveChartsCore.SkiaSharpView.Painting;

namespace Finance_Tracker_WPF_API.UI.Converters
{
    public class ExpenseCategoryDoughnutChartConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var pastelPalette = new[]
            {
                SKColor.Parse("#A7C7E7"),
                SKColor.Parse("#FFD6E0"),
                SKColor.Parse("#FFE5B4"),
                SKColor.Parse("#B5EAD7"),
                SKColor.Parse("#C7CEEA"),
                SKColor.Parse("#FFFACD"),
                SKColor.Parse("#FFDAC1"),
                SKColor.Parse("#E2F0CB"), 
                SKColor.Parse("#B5B9FF"), 
                SKColor.Parse("#E0BBE4"), 
            };

            if (value is IEnumerable<object> transactions && transactions.Any())
            {
                var expenseTotals = transactions
                    .Select(t => new {
                        Type = t.GetType().GetProperty("Type")?.GetValue(t)?.ToString(),
                        CategoryName = t.GetType().GetProperty("CategoryName")?.GetValue(t)?.ToString(),
                        Amount = t.GetType().GetProperty("AmountInSelectedCurrency")?.GetValue(t) ?? 0m
                    })
                    .Where(x => x.Type == "Expense")
                    .GroupBy(x => string.IsNullOrWhiteSpace(x.CategoryName) ? "Без категории" : x.CategoryName)
                    .Select(g => new { Category = g.Key, Total = g.Sum(x => (decimal)x.Amount) })
                    .Where(x => x.Total > 0)
                    .ToList();

                if (expenseTotals.Any())
                {
                    var total = expenseTotals.Sum(x => x.Total);
                    return expenseTotals.Select((x, i) =>
                        new PieSeries<decimal>
                        {
                            Values = new[] { x.Total },
                            Name = x.Category,
                            InnerRadius = 50,
                            DataLabelsSize = 14,
                            DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Outer,
                            DataLabelsFormatter = point => $"{x.Category}: {Math.Round(x.Total / total * 100, 2):F2}% ({x.Total:F2})",
                            Fill = new SolidColorPaint(pastelPalette[i % pastelPalette.Length])
                        }
                    ).ToArray();
                }
            }

            return new ISeries[]
            {
                new PieSeries<decimal>
                {
                    Values = new decimal[] { 1 },
                    Name = "No Data",
                    InnerRadius = 50,
                    DataLabelsSize = 14,
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Outer,
                    DataLabelsFormatter = _ => "No Data",
                    Fill = new SolidColorPaint(SKColors.LightGray)
                }
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
}
