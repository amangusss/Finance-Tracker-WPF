using System.Globalization;
using System.Windows.Data;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace Finance_Tracker_WPF_API.UI.Converters;

public class CategoryPieChartConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not Dictionary<string, decimal> categoryTotals)
            return Array.Empty<ISeries>();

        var series = new List<ISeries>();
        var random = new Random();
        var colors = new[]
        {
            SKColors.DodgerBlue,
            SKColors.ForestGreen,
            SKColors.Orange,
            SKColors.Red,
            SKColors.Purple,
            SKColors.Teal,
            SKColors.Gray
        };

        var colorIndex = 0;
        foreach (var category in categoryTotals)
        {
            series.Add(new PieSeries<double>
            {
                Name = category.Key,
                Values = new[] { (double)category.Value },
                Fill = new SolidColorPaint(colors[colorIndex % colors.Length])
            });
            colorIndex++;
        }

        return series;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
} 