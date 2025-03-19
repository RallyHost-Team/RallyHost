using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace RallyHost.Converters
{
    public class ToggleButtonCheckedConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return string.Equals(value?.ToString(), parameter?.ToString(), StringComparison.Ordinal);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b && b)
                return parameter;
            return null;
        }
    }
}