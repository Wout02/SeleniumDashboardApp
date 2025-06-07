namespace SeleniumDashboardApp.Views.Tabs;

public partial class LogsView : ContentView
{
    public LogsView()
    {
        InitializeComponent();

        this.BindingContextChanged += (_, __) =>
        {
            System.Diagnostics.Debug.WriteLine($"[LOGSVIEW] BindingContext updated to: {BindingContext?.GetType().Name ?? "null"}");
        };
    }
}
