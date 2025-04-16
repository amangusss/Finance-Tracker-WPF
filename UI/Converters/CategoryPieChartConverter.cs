using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Finance_Tracker_WPF_API.Core.Models;

namespace Finance_Tracker_WPF_API.UI.Converters
{
    public class CategoryPieChartConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<Transaction> transactions && transactions.Any())
            {
                var categoryTotals = transactions
                    .Where(t => t.Category != null) 
                    .GroupBy(t => t.Category.Name)
                    .Select(g => new
                    {
                        Category = g.Key,
                        Total = g.Sum(t => t.Type == TransactionType.Income ? t.Amount : -t.Amount)
                    })
                    .Where(x => x.Total != 0)
                    .OrderByDescending(x => Math.Abs(x.Total))
                    .ToList();

                if (categoryTotals.Any())
                {
                    return new ISeries[]
                    {
                        new PieSeries<double>
                        {
                            Values = categoryTotals.Select(x => (double)Math.Abs(x.Total)).ToArray(),
                            Name = "Categories",
                            DataLabelsSize = 14,
                            DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Outer,
                            DataLabelsFormatter = (point) => $"{categoryTotals[point.Index].Category}: {categoryTotals[point.Index].Total:C}"
                        }
                    };
                }
            }

            return new ISeries[]
            {
                new PieSeries<double>
                {
                    Values = new double[] { 1 },
                    Name = "No Data",
                    DataLabelsSize = 14,
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Outer,
                    DataLabelsFormatter = (point) => "No transactions"
                }
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}