using System.Globalization;
using System.Windows.Data;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace Finance_Tracker_WPF_API.UI.Converters
{
    public class DailyIncomeExpenseLineChartConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<object> items)
            {
                var grouped = items.GroupBy(item => ((DateTime)item.GetType().GetProperty("Date")?.GetValue(item)).Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new
                    {
                        Date = g.Key,
                        Income = g.Where(t => t.GetType().GetProperty("Type")?.GetValue(t)?.ToString() == nameof(Finance_Tracker_WPF_API.Core.Models.TransactionType.Income)).Sum(t => (decimal)(t.GetType().GetProperty("AmountInSelectedCurrency")?.GetValue(t) ?? 0m)),
                        Expense = g.Where(t => t.GetType().GetProperty("Type")?.GetValue(t)?.ToString() == nameof(Finance_Tracker_WPF_API.Core.Models.TransactionType.Expense)).Sum(t => (decimal)(t.GetType().GetProperty("AmountInSelectedCurrency")?.GetValue(t) ?? 0m))
                    }).ToList();

                var incomeSeries = new LineSeries<decimal>
                {
                    Name = "Income",
                    Values = grouped.Select(x => x.Income).ToArray(),
                    GeometrySize = 16,
                    Stroke = new SolidColorPaint(SKColors.Black, 2),
                    Fill = null
                };
                var expenseSeries = new LineSeries<decimal>
                {
                    Name = "Expense",
                    Values = grouped.Select(x => x.Expense).ToArray(),
                    GeometrySize = 16,
                    Stroke = new SolidColorPaint(SKColors.Black, 2),
                    Fill = null
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
