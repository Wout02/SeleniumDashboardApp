using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;
using SeleniumDashboard.Shared;
using SeleniumDashboardApp.Services;
using Microcharts;
using SkiaSharp;
using System.Diagnostics;

namespace SeleniumDashboardApp.ViewModels;

public partial class TestRunDetailViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    private TestRun? selectedTestRun;

    [ObservableProperty]
    private bool showAggregateData;

    [ObservableProperty]
    private Chart? barChart1;

    [ObservableProperty]
    private Chart? barChart2;

    [ObservableProperty]
    private Chart? barChart3;


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

    public TestRunDetailViewModel(ApiService apiService)
    {
        _apiService = apiService;
        ChangeTabCommand = new Command<string>(tab => SelectedTab = tab);
    }

    public async Task LoadTestRunById(int id)
    {
        var testRun = await _apiService.GetTestRunAsync(id);

        if (testRun != null)
        {
            if (string.IsNullOrWhiteSpace(testRun.LogOutput))
            {
                // Tijdelijke dummydata
                testRun.LogOutput = "[PASSED] LoginTest\n[FAILED] CheckoutTest\n[PASSED] SearchTest\n[PASSED] LogoutTest";
                Debug.WriteLine("[DEBUG] Dummy logoutput toegevoegd");
            }

            SelectedTestRun = testRun;
        }
        else
        {
            Debug.WriteLine("[DEBUG] Geen testRun ontvangen van API");
        }
    }

    public async Task LoadChartsAsync()
    {
        Debug.WriteLine($"[CHARTS] LoadChartsAsync - ShowAggregateData: {ShowAggregateData}");

        if (ShowAggregateData)
            await LoadChartsFromAllTestRunsAsync();
        else
            await LoadChartsFromTestRunAsync();
    }

    private async Task LoadChartsFromTestRunAsync()
    {
        if (SelectedTestRun?.LogOutput == null)
        {
            Debug.WriteLine("[CHARTS] Geen LogOutput beschikbaar voor geselecteerde testrun");
            return;
        }

        Debug.WriteLine("[CHARTS] LogOutput van geselecteerde run:");
        Debug.WriteLine(SelectedTestRun.LogOutput);

        var lines = SelectedTestRun.LogOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        int passed = lines.Count(l => l.TrimStart().StartsWith("✔"));
        int failed = lines.Count(l => l.TrimStart().StartsWith("×"));

        Debug.WriteLine($"[CHARTS] Passed: {passed}, Failed: {failed}");

        var entries = new List<ChartEntry>
        {
            new(passed) { Label = "Passed", ValueLabel = passed.ToString(), Color = SKColors.Green },
            new(failed) { Label = "Failed", ValueLabel = failed.ToString(), Color = SKColors.Red }
        };

        BarChart1 = new BarChart { Entries = entries, LabelTextSize = 14f };
        BarChart2 = new PointChart { Entries = entries, LabelTextSize = 14f };
        BarChart3 = new LineChart { Entries = entries, LabelTextSize = 14f };
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

                foreach (var line in lines)
                {
                    var trimmed = line.TrimStart();
                    if (trimmed.StartsWith("✔"))
                        totalPassed++;
                    else if (trimmed.StartsWith("×"))
                        totalFailed++;
                }
            }
        }

        Debug.WriteLine($"[CHARTS AGGREGATE] Total Passed: {totalPassed}, Total Failed: {totalFailed}");

        var entries = new List<ChartEntry>
    {
        new(totalPassed) { Label = "Passed", ValueLabel = totalPassed.ToString(), Color = SKColors.Green },
        new(totalFailed) { Label = "Failed", ValueLabel = totalFailed.ToString(), Color = SKColors.Red }
    };

        BarChart1 = new BarChart { Entries = entries, LabelTextSize = 14f };
        BarChart2 = new PointChart { Entries = entries, LabelTextSize = 14f };
        BarChart3 = new LineChart { Entries = entries, LabelTextSize = 14f };
    }


    partial void OnShowAggregateDataChanged(bool value)
    {
        // Reageer op toggle switch direct via databinding
        _ = LoadChartsAsync();
    }
}