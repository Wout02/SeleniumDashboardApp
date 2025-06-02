using Microsoft.Maui.Controls;
using System;
using System.Globalization;

namespace SeleniumDashboardApp.Converters;

public class TabStyleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string selectedTab = value?.ToString() ?? "";
        string currentTab = parameter?.ToString() ?? "";

        return selectedTab == currentTab
            ? Application.Current.Resources["TabButtonActiveStyle"]
            : Application.Current.Resources["TabButtonBaseStyle"];
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}