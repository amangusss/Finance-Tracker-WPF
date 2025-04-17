using System.Globalization;
using System.Windows.Data;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using Finance_Tracker_WPF_API.Core.Models;

namespace Finance_Tracker_WPF_API.UI.Converters
{
    public class IncomeExpensePieChartConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<object> items)
            {
                decimal incomeSum = 0, expenseSum = 0;
                foreach (var item in items)
                {
                    var typeProp = item.GetType().GetProperty("Type");
                    var amountProp = item.GetType().GetProperty("AmountInSelectedCurrency");
                    if (typeProp != null && amountProp != null)
                    {
                        var typeValue = typeProp.GetValue(item)?.ToString();
                        var amount = (decimal)(amountProp.GetValue(item) ?? 0m);
                        if (typeValue == nameof(TransactionType.Income))
                            incomeSum += amount;
                        else if (typeValue == nameof(TransactionType.Expense))
                            expenseSum += amount;
                    }
                }
                if (incomeSum == 0 && expenseSum == 0)
                {
                    return new ISeries[]
                    {
                        new PieSeries<decimal>
                        {
                            Values = new decimal[] { 0 },
                            Name = "No Data"
                        }
                    };
                }
                return new ISeries[]
                {
                    new PieSeries<decimal>
                    {
                        Values = new decimal[] { Math.Round(incomeSum, 2) },
                        Name = "Income",
                        Fill = new SolidColorPaint(SKColors.Green)
                    },
                    new PieSeries<decimal>
                    {
                        Values = new decimal[] { Math.Round(expenseSum, 2) },
                        Name = "Expense",
                        Fill = new SolidColorPaint(SKColors.Red)
                    }
                };
            }
            return new ISeries[]
            {
                new PieSeries<decimal>
                {
                    Values = new decimal[] { 0 },
                    Name = "No Data"
                }
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
