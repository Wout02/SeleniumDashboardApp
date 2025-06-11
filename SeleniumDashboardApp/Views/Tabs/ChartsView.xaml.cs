using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
using SeleniumDashboardApp.ViewModels;

namespace SeleniumDashboardApp.Views.Tabs;

public partial class ChartsView : ContentView
{
    public ChartsView()
    {
        InitializeComponent();
        this.BindingContextChanged += (_, __) =>
        {
            System.Diagnostics.Debug.WriteLine($"[LOGSVIEW] BindingContext updated to: {BindingContext?.GetType().Name ?? "null"}");
        };
        Console.WriteLine("[CHARTS VIEW] Constructor aangeroepen");
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        Console.WriteLine("[CHARTS VIEW] BindingContext veranderd");

        if (BindingContext is TestRunDetailViewModel vm)
        {
            Console.WriteLine("[CHARTS VIEW] BindingContext is geldig ViewModel");
            _ = vm.LoadChartsAsync();
        }
    }

    private async void OnToggleChartScope(object sender, ToggledEventArgs e)
    {
        if (BindingContext is TestRunDetailViewModel vm)
        {
            await vm.LoadChartsAsync();
        }
    }
}