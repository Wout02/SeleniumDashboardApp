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

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        Console.WriteLine("[CHARTS VIEW] BindingContext veranderd");

        if (BindingContext is TestRunDetailViewModel vm)
        {
            Console.WriteLine("[CHARTS VIEW] BindingContext is geldig ViewModel");

            // Laad grafieken bij eerste bind
            _ = vm.LoadChartsAsync();

            // Luister naar veranderingen aan ShowAggregateData zodat we kunnen herladen
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