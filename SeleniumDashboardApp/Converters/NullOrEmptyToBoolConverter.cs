using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace SeleniumDashboardApp.Converters
{
    public class NullOrEmptyToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isNotNullOrEmpty = !string.IsNullOrWhiteSpace(value?.ToString());

            if (parameter?.ToString()?.ToLower() == "invert")
                return !isNotNullOrEmpty;

            return isNotNullOrEmpty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}