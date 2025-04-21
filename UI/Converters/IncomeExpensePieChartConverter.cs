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
            var pastelPalette = new[]
            {
                SKColor.Parse("#B5EAD7"),
                SKColor.Parse("#FFD6E0") 
            };
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
                
                var total = incomeSum + expenseSum;
                if (total > 0)
                {
                    var incomePercent = Math.Round(incomeSum / total * 100, 2);
                    var expensePercent = Math.Round(expenseSum / total * 100, 2);
                    return new ISeries[]
                    {
                        new PieSeries<decimal>
                        {
                            Values = new decimal[] { Math.Round(incomeSum, 2) },
                            Name = "Income",
                            Fill = new SolidColorPaint(pastelPalette[0]),
                            DataLabelsSize = 14,
                            DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Outer,
                            DataLabelsFormatter = point => $"{incomePercent:F2}%",
                            ToolTipLabelFormatter = point => $"{incomePercent:F2}%"
                        },
                        new PieSeries<decimal>
                        {
                            Values = new decimal[] { Math.Round(expenseSum, 2) },
                            Name = "Expense",
                            Fill = new SolidColorPaint(pastelPalette[1]),
                            DataLabelsSize = 14,
                            DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Outer,
                            DataLabelsFormatter = point => $"{expensePercent:F2}%",
                            ToolTipLabelFormatter = point => $"{expensePercent:F2}%"
                        }
                    };
                }
            }
            return new ISeries[]
            {
                new PieSeries<decimal>
                {
                    Values = new decimal[] { 0 },
                    Name = "No Data",
                    Fill = new SolidColorPaint(SKColors.LightGray)
                }
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
