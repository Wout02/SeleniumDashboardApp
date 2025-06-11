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

    [ObservableProperty] private TestRun? selectedTestRun;
    [ObservableProperty] private bool showAggregateData;

    [ObservableProperty] private Chart? barChart1;
    [ObservableProperty] private Chart? barChart2;
    [ObservableProperty] private Chart? barChart3;
    [ObservableProperty] private Chart? barChart4;

    private string _selectedTab = "Details";
    public string SelectedTab
    {
        get => _selectedTab;
        set
        {
            if (SetProperty(ref _selectedTab, value) && value == "Charts")
            {
                _ = LoadChartsAsync();
            }
        }
    }

    public ICommand ChangeTabCommand { get; }
    public ICommand ToggleChartDataScopeCommand { get; }

    public TestRunDetailViewModel(ApiService apiService)
    {
        _apiService = apiService;
        ChangeTabCommand = new Command<string>(tab => SelectedTab = tab);
        ToggleChartDataScopeCommand = new Command(async () =>
        {
            ShowAggregateData = !ShowAggregateData;
            await LoadChartsAsync();
        });
    }

    public async Task LoadTestRunById(int id)
    {
        var testRun = await _apiService.GetTestRunAsync(id);
        if (testRun != null)
        {
            SelectedTestRun = testRun;
        }
    }

    public async Task LoadChartsAsync()
    {
        if (ShowAggregateData)
            await LoadChartsFromAllTestRunsAsync();
        else
            await LoadChartsFromTestRunAsync();
    }

    private async Task LoadChartsFromTestRunAsync()
    {
        if (SelectedTestRun?.LogOutput == null) return;

        var lines = SelectedTestRun.LogOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        int passed = lines.Count(l => l.Contains("[PASSED]", StringComparison.OrdinalIgnoreCase));
        int failed = lines.Count(l => l.Contains("[FAILED]", StringComparison.OrdinalIgnoreCase));

        var entries = new List<ChartEntry>
        {
            new(passed) { Label = "Passed", ValueLabel = passed.ToString(), Color = SKColors.Green },
            new(failed) { Label = "Failed", ValueLabel = failed.ToString(), Color = SKColors.Red }
        };

        barChart1 = new BarChart { Entries = entries, LabelTextSize = 14f };
        barChart2 = new PointChart { Entries = entries, LabelTextSize = 14f };
        barChart3 = new LineChart { Entries = entries, LabelTextSize = 14f };
        barChart4 = new DonutChart { Entries = entries, LabelTextSize = 14f };
    }

    private async Task LoadChartsFromAllTestRunsAsync()
    {
        var testRuns = await _apiService.GetTestRunsAsync();
        int totalPassed = 0, totalFailed = 0;

        foreach (var run in testRuns)
        {
            if (!string.IsNullOrWhiteSpace(run.LogOutput))
            {
                var lines = run.LogOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                totalPassed += lines.Count(l => l.Contains("[PASSED]", StringComparison.OrdinalIgnoreCase));
                totalFailed += lines.Count(l => l.Contains("[FAILED]", StringComparison.OrdinalIgnoreCase));
            }
        }

        var entries = new List<ChartEntry>
        {
            new(totalPassed) { Label = "Passed", ValueLabel = totalPassed.ToString(), Color = SKColors.Green },
            new(totalFailed) { Label = "Failed", ValueLabel = totalFailed.ToString(), Color = SKColors.Red }
        };

        barChart1 = new BarChart { Entries = entries, LabelTextSize = 14f };
        barChart2 = new PointChart { Entries = entries, LabelTextSize = 14f };
        barChart3 = new LineChart { Entries = entries, LabelTextSize = 14f };
        barChart4 = new DonutChart { Entries = entries, LabelTextSize = 14f };
    }
}