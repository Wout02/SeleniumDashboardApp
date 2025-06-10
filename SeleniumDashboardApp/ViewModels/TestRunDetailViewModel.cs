using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;
using SeleniumDashboard.Shared;
using SeleniumDashboardApp.Services;
using Microcharts;
using SkiaSharp;

namespace SeleniumDashboardApp.ViewModels;

public partial class TestRunDetailViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    private TestRun? selectedTestRun;

    [ObservableProperty]
    private Chart? barChart;

    private string _selectedTab = "Details";
    public string SelectedTab
    {
        get => _selectedTab;
        set
        {
            if (SetProperty(ref _selectedTab, value))
            {
                if (value == "Charts" && BarChart == null)
                {
                    CreateTestChart();
                }
            }
        }
    }

    public ICommand ChangeTabCommand { get; }

    public TestRunDetailViewModel(ApiService apiService)
    {
        _apiService = apiService;
        ChangeTabCommand = new Command<string>(tab => SelectedTab = tab);
    }

    public async Task LoadTestRunById(int id)
    {
        Console.WriteLine($"[DEBUG] Ophalen van testrun met ID {id}");
        var testRun = await _apiService.GetTestRunAsync(id);
        Console.WriteLine($"[DEBUG] API-response: {(testRun != null ? "OK" : "null")}");
        if (testRun != null)
        {
            SelectedTestRun = testRun;
            Console.WriteLine($"[DEBUG] Summary: {testRun.Summary}");
            Console.WriteLine($"[DEBUG] LogOutput: {testRun.LogOutput}");
        }
    }

    private void CreateTestChart()
    {
        var entries = new[]
        {
            new ChartEntry(5)
            {
                Label = "Passed",
                ValueLabel = "5",
                Color = SKColors.Green
            },
            new ChartEntry(2)
            {
                Label = "Failed",
                ValueLabel = "2",
                Color = SKColors.Red
            }
        };

        BarChart = new BarChart
        {
            Entries = entries,
            LabelTextSize = 16f,
            ValueLabelTextSize = 14f,
            BackgroundColor = SKColors.Transparent
        };
    }
}