using Microsoft.Maui.Controls;
using System;
using SeleniumDashboardApp.ViewModels;

namespace SeleniumDashboardApp.Views.Tabs;

public partial class ChartsView : ContentView
{
    public ChartsView()
    {
        InitializeComponent();
        this.BindingContextChanged += (_, __) =>
        {
            System.Diagnostics.Debug.WriteLine($"[CHARTSVIEW] BindingContext updated to: {BindingContext?.GetType().Name ?? "null"}");
        };
        Console.WriteLine("[CHARTS VIEW] Constructor aangeroepen");
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        if (width <= 400)
            VisualStateManager.GoToState(this, "Mobile");
        else
            VisualStateManager.GoToState(this, "DesktopOrTablet");
    }


    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        Console.WriteLine("[CHARTS VIEW] BindingContext veranderd");

        if (BindingContext is TestRunDetailViewModel vm)
        {
            Console.WriteLine("[CHARTS VIEW] BindingContext is geldig ViewModel");

            _ = vm.LoadChartsAsync();

            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(vm.ShowAggregateData))
                {
                    Console.WriteLine("[CHARTS VIEW] ShowAggregateData gewijzigd → grafieken opnieuw laden");
                    _ = vm.LoadChartsAsync();
                }
            };
        }
    }
}