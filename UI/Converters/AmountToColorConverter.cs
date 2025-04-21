using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Finance_Tracker_WPF_API.UI.Converters
{
    public class AmountToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal amount)
            {
                if (amount > 0)
                    return new SolidColorBrush(Colors.Green);
                if (amount < 0)
                    return new SolidColorBrush(Colors.Red);
                return new SolidColorBrush(Colors.Gray);
            }

            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
