using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace SeleniumDashboardApp.Converters
{
    public class StatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString()?.ToLower() switch
            {
                "passed" => Colors.Green,
                "failed" => Colors.Red,
                _ => Colors.Gray
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
