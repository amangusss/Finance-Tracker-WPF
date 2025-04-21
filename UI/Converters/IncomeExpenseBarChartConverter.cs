using System.Globalization;
using System.Windows.Data;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace Finance_Tracker_WPF_API.UI.Converters
{
    public class IncomeExpenseBarChartConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var pastelPalette = new[]
            {
                SKColor.Parse("#B5EAD7"),
                SKColor.Parse("#FFD6E0") 
            };
            if (value is IEnumerable<object> items)
            {
                var grouped = items.GroupBy(item => ((DateTime)item.GetType().GetProperty("Date")?.GetValue(item)).Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new
                    {
                        Date = g.Key,
                        Income = Math.Round(g.Where(t => t.GetType().GetProperty("Type")?.GetValue(t)?.ToString() == "Income").Sum(t => (decimal)(t.GetType().GetProperty("AmountInSelectedCurrency")?.GetValue(t) ?? 0m)), 2),
                        Expense = Math.Round(g.Where(t => t.GetType().GetProperty("Type")?.GetValue(t)?.ToString() == "Expense").Sum(t => (decimal)(t.GetType().GetProperty("AmountInSelectedCurrency")?.GetValue(t) ?? 0m)), 2)
                    }).ToList();

                var incomeSeries = new ColumnSeries<decimal>
                {
                    Name = "Income",
                    Values = grouped.Select(x => Math.Round(x.Income, 2)).ToArray(),
                    Fill = new SolidColorPaint(pastelPalette[0]),
                    DataLabelsFormatter = point => point.PrimaryValue.ToString("N2")
                };
                var expenseSeries = new ColumnSeries<decimal>
                {
                    Name = "Expense",
                    Values = grouped.Select(x => Math.Round(x.Expense, 2)).ToArray(),
                    Fill = new SolidColorPaint(pastelPalette[1]),
                    DataLabelsFormatter = point => point.PrimaryValue.ToString("N2")
                };
                return new ISeries[] { incomeSeries, expenseSeries };
            }
            return new ISeries[0];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}