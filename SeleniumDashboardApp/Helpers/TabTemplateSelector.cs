using Microsoft.Maui.Controls;

namespace SeleniumDashboardApp.Helpers;

public class TabTemplateSelector : DataTemplateSelector
{
    public DataTemplate DetailsTemplate { get; set; }
    public DataTemplate LogsTemplate { get; set; }
    public DataTemplate ChartsTemplate { get; set; }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        if (item?.ToString() == "Charts")
        {
            return ChartsTemplate;
        }

        if (item?.ToString() == "Logs")
        {
            return LogsTemplate;
        }

        return DetailsTemplate;
    }
}