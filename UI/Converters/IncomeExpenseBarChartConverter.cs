using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Finance_Tracker_WPF_API.Core.Models;
using SkiaSharp;

namespace Finance_Tracker_WPF_API.UI.Converters
{
    public class IncomeExpenseBarChartConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<Transaction> transactions)
            {
                var grouped = transactions
                    .GroupBy(t => new { t.Date.Year, t.Date.Month })
                    .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                    .ToList();

                if (grouped.Count == 0)
                {
                    return new ISeries[]
                    {
                        new ColumnSeries<decimal>
                        {
                            Values = new decimal[] { 0 },
                            Name = "No Data"
                        }
                    };
                }

                var income = grouped.Select(g => g.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount)).ToArray();
                var expense = grouped.Select(g => g.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount)).ToArray();

                // Если нет доходов или расходов, делаем массивы нулей нужной длины
                if (income.Length == 0) income = Enumerable.Repeat(0m, grouped.Count).ToArray();
                if (expense.Length == 0) expense = Enumerable.Repeat(0m, grouped.Count).ToArray();

                return new ISeries[]
                {
                    new ColumnSeries<decimal>
                    {
                        Values = income,
                        Name = "Income",
                        Fill = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint(SKColors.Green)
                    },
                    new ColumnSeries<decimal>
                    {
                        Values = expense,
                        Name = "Expense",
                        Fill = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint(SKColors.Red)
                    }
                };
            }

            return new ISeries[]
            {
                new ColumnSeries<decimal>
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