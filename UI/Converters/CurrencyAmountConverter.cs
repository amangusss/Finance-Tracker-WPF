using System.Globalization;
using System.Windows.Data;

namespace Finance_Tracker_WPF_API.UI.Converters
{
    public class CurrencyAmountConverter : IMultiValueConverter
    {
        private static readonly Dictionary<string, string> CurrencySymbols = new()
        {
            { "USD", "$" },
            { "EUR", "€" },
            { "RUB", "₽" },
            { "KZT", "₸" },
            { "UAH", "₴" },
            { "KGS", "KGS" },
            { "GBP", "£" }
        };

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is decimal amount && values[1] is string currency)
            {
                string symbol = CurrencySymbols.TryGetValue(currency, out var s) ? s : currency;
                return string.Format("{0:N2} {1}", amount, symbol);
            }
            return Binding.DoNothing;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            if (value is string s)
            {
                var parts = s.Split(' ');
                if (parts.Length >= 2 && decimal.TryParse(parts[0], NumberStyles.Number, culture, out var amount))
                    return new object[] { amount, parts[1] };
            }
            var result = new object[targetTypes.Length];
            for (int i = 0; i < result.Length; i++)
                result[i] = Binding.DoNothing;
            return result;
        }
    }
}
