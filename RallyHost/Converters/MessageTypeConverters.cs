using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using RallyHost.Controls;

namespace RallyHost.Converters
{
    public class MessageTypeConverters : IMultiValueConverter
    {
        public static readonly MessageTypeConverters Instance = new();

        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is MessageType type)
            {
                return type switch
                {
                    MessageType.Information => Geometry.Parse("M12,2C6.48,2,2,6.48,2,12s4.48,10,10,10s10-4.48,10-10S17.52,2,12,2z M13,17h-2v-6h2V17z M13,9h-2V7h2V9z"),
                    MessageType.Warning => Geometry.Parse("M1,21h22L12,2L1,21z M13,18h-2v-2h2V18z M13,14h-2V9h2V14z"),
                    MessageType.Error => Geometry.Parse("M12,2C6.48,2,2,6.48,2,12s4.48,10,10,10s10-4.48,10-10S17.52,2,12,2z M13,17h-2v-2h2V17z M13,13h-2V7h2V13z"),
                    MessageType.Success => Geometry.Parse("M12,2C6.48,2,2,6.48,2,12s4.48,10,10,10s10-4.48,10-10S17.52,2,12,2z M10,17l-5-5l1.41-1.41L10,14.17l7.59-7.59L19,8L10,17z"),
                    MessageType.Question => Geometry.Parse("M12,2C6.48,2,2,6.48,2,12s4.48,10,10,10s10-4.48,10-10S17.52,2,12,2z M13,19h-2v-2h2V19z M15.07,11.25l-0.9,0.92C13.45,12.9,13,13.5,13,15h-2v-0.5c0-1.1,0.45-2.1,1.17-2.83l1.24-1.26c0.37-0.36,0.59-0.86,0.59-1.41c0-1.1-0.9-2-2-2s-2,0.9-2,2H8c0-2.21,1.79-4,4-4s4,1.79,4,4C16,9.67,15.65,10.56,15.07,11.25z"),
                    _ => Geometry.Parse("M12,2C6.48,2,2,6.48,2,12s4.48,10,10,10s10-4.48,10-10S17.52,2,12,2z M13,17h-2v-6h2V17z M13,9h-2V7h2V9z"),
                };
            }
            return null;
        }
    }

    public class MessageTypeToBrushConverter : IMultiValueConverter
    {
        public static readonly MessageTypeToBrushConverter Instance = new();

        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is MessageType type)
            {
                return type switch
                {
                    MessageType.Information => new SolidColorBrush(Color.Parse("#007BFF")),
                    MessageType.Warning => new SolidColorBrush(Color.Parse("#FFC107")),
                    MessageType.Error => new SolidColorBrush(Color.Parse("#DC3545")),
                    MessageType.Success => new SolidColorBrush(Color.Parse("#28A745")),
                    MessageType.Question => new SolidColorBrush(Color.Parse("#6C757D")),
                    _ => new SolidColorBrush(Color.Parse("#007BFF")),
                };
            }
            return null;
        }
    }
}