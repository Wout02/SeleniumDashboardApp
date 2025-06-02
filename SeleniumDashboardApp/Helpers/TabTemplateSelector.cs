using Microsoft.Maui.Controls;

namespace SeleniumDashboardApp.Helpers;

public class TabTemplateSelector : DataTemplateSelector
{
    public DataTemplate DetailsTemplate { get; set; }
    public DataTemplate LogsTemplate { get; set; }
    public DataTemplate ChartsTemplate { get; set; }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        var tab = item as string;
        return tab switch
        {
            "Details" => DetailsTemplate,
            "Logs" => LogsTemplate,
            "Charts" => ChartsTemplate,
            _ => DetailsTemplate,
        };
    }
}