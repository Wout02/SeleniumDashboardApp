using Microsoft.Maui.Controls;

namespace SeleniumDashboardApp.Helpers;

public class TabTemplateSelector : DataTemplateSelector
{
    public DataTemplate DetailsTemplate { get; set; }
    public DataTemplate LogsTemplate { get; set; }
    public DataTemplate ChartsTemplate { get; set; }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        Console.WriteLine($"[TAB TEMPLATE SELECTOR] Item = {item}");

        if (item?.ToString() == "Charts")
        {
            Console.WriteLine("[TAB TEMPLATE SELECTOR] ChartsTemplate wordt gebruikt");
            return ChartsTemplate;
        }

        if (item?.ToString() == "Logs")
        {
            Console.WriteLine("[TAB TEMPLATE SELECTOR] LogsTemplate wordt gebruikt");
            return LogsTemplate;
        }

        Console.WriteLine("[TAB TEMPLATE SELECTOR] Fallback naar DetailsTemplate");
        return DetailsTemplate;
    }
}