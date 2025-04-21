using System.Collections;
using System.Globalization;
using System.Windows.Data;
using Finance_Tracker_WPF_API.Core.Models;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace Finance_Tracker_WPF_API.UI.Converters
{
    public class ExpenseCategoryDoughnutChartMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var pastelPalette = new[]
            {
                SKColor.Parse("#A7C7E7"), SKColor.Parse("#FFD6E0"), SKColor.Parse("#FFE5B4"), SKColor.Parse("#B5EAD7"),
                SKColor.Parse("#C7CEEA"), SKColor.Parse("#FFFACD"), SKColor.Parse("#FFDAC1"), SKColor.Parse("#E2F0CB"),
                SKColor.Parse("#B5B9FF"), SKColor.Parse("#E0BBE4"),
            };

            var transactionsEnumerable = values[0] as IEnumerable;
            var categories = values[1] as IEnumerable;
            if (transactionsEnumerable == null || categories == null)
            {
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
            var transactions = transactionsEnumerable.Cast<object>();

            var filtered = new Dictionary<string, decimal>();
            if (transactions != null)
            {
                foreach (var t in transactions)
                {
                    if (t is Transaction tx &&
                        tx.Category != null &&
                        tx.Type == TransactionType.Expense)
                    {
                        var catName = tx.Category.Name;
                        if (!filtered.ContainsKey(catName))
                            filtered[catName] = 0;
                        filtered[catName] += Math.Abs(tx.Amount);
                    }
                }
            }

            var result = new List<ISeries>();
            int i = 0;
            foreach (var pair in filtered.Where(p => p.Value > 0))
            {
                result.Add(new PieSeries<decimal>
                {
                    Values = new[] { pair.Value },
                    Name = pair.Key,
                    InnerRadius = 50,
                    DataLabelsSize = 14,
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Outer,
                    DataLabelsFormatter = point => $"{pair.Key}: {pair.Value:F2}",
                    Fill = new SolidColorPaint(pastelPalette[i % pastelPalette.Length])
                });
                i++;
            }
            if (result.Count > 0)
                return result.ToArray();
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

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            var result = new object[targetTypes.Length];
            for (int i = 0; i < result.Length; i++)
                result[i] = Binding.DoNothing;
            return result;
        }
    }
}
