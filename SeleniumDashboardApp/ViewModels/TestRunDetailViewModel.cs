using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;
using SeleniumDashboard.Shared;
using SeleniumDashboardApp.Services;
using Microcharts;
using SkiaSharp;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace SeleniumDashboardApp.ViewModels;

public partial class TestRunDetailViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    [ObservableProperty] private TestRun? selectedTestRun;
    [ObservableProperty] private bool showAggregateData;

    [ObservableProperty] private Chart? barChart1;
    [ObservableProperty] private Chart? barChart2;
    [ObservableProperty] private Chart? barChart3;

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
                testRun.LogOutput = "✔ Passed test\n× Failed test\n✔ Another passed";
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
        if (SelectedTestRun?.LogOutput == null) return;

        var lines = SelectedTestRun.LogOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // === BAR CHART 1: PASSED / FAILED TOTAL ===
        int passed = lines.Count(l => l.TrimStart().StartsWith("✔"));
        int failed = lines.Count(l => l.TrimStart().StartsWith("×"));

        var passedFailedEntries = new List<ChartEntry>
    {
        new(passed) { Label = "Passed", ValueLabel = passed.ToString(), Color = SKColors.Green },
        new(failed) { Label = "Failed", ValueLabel = failed.ToString(), Color = SKColors.Red }
    };

        BarChart1 = new BarChart
        {
            Entries = passedFailedEntries,
            LabelTextSize = 14f,
            BackgroundColor = SKColors.White
        };

        // === POINT CHART: TIME PER TEST ===
        var timeEntries = lines
            .Where(l => l.Contains("ms"))
            .Select((line, index) =>
            {
                var match = Regex.Match(line, @"(.+?)\((\d+)ms\)");
                if (!match.Success) return null;

                int ms = int.Parse(match.Groups[2].Value);
                return new ChartEntry(ms)
                {
                    Label = $"{index + 1}", // alleen cijfer
                    ValueLabel = $"{ms}ms",
                    Color = SKColors.Orange
                };
            })
            .Where(e => e != null)
            .ToList()!;

        BarChart2 = new PointChart
        {
            Entries = timeEntries,
            LabelTextSize = 14f,
            BackgroundColor = SKColors.White
        };

        // === LINE CHART: TESTFLOW ✔ = 1, × = 0 ===
        var flowEntries = new List<ChartEntry>();
        int step = 1;

        foreach (var line in lines.Where(l => l.TrimStart().StartsWith("✔") || l.TrimStart().StartsWith("×")))
        {
            bool isPassed = line.TrimStart().StartsWith("✔");

            flowEntries.Add(new ChartEntry(isPassed ? 1 : 0)
            {
                Label = $"Step {step++}",
                ValueLabel = "", // leeg label boven de stip
                Color = isPassed ? SKColors.Green : SKColors.Red
            });
        }

        BarChart3 = new LineChart
        {
            Entries = flowEntries,
            LabelTextSize = 14f,
            BackgroundColor = SKColors.White,
            LineMode = LineMode.Straight,
            PointMode = PointMode.Circle,
            MinValue = 0,
            MaxValue = 1
        };
    }

    private async Task LoadChartsFromAllTestRunsAsync()
    {
        var testRuns = await _apiService.GetTestRunsAsync();

        var grouped = testRuns
            .Where(r => !string.IsNullOrWhiteSpace(r.LogOutput))
            .GroupBy(r => r.Date.Date)
            .OrderBy(g => g.Key);

        var passedEntries = new List<ChartEntry>();
        var failedEntries = new List<ChartEntry>();

        foreach (var group in grouped)
        {
            int passed = 0, failed = 0;

            foreach (var run in group)
            {
                var lines = run.LogOutput!.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                passed += lines.Count(l => l.TrimStart().StartsWith("✔"));
                failed += lines.Count(l => l.TrimStart().StartsWith("×"));
            }

            var label = group.Key.ToString("MM-dd");

            passedEntries.Add(new ChartEntry(passed) { Label = label, ValueLabel = passed.ToString(), Color = SKColors.Green });
            failedEntries.Add(new ChartEntry(failed) { Label = label, ValueLabel = failed.ToString(), Color = SKColors.Red });
        }

        BarChart1 = new BarChart
        {
            Entries = passedEntries.Concat(failedEntries).ToList(),
            LabelTextSize = 14f,
            Margin = 10,
            BackgroundColor = SKColors.White
        };

        BarChart2 = new LineChart
        {
            Entries = passedEntries,
            LabelTextSize = 14f,
            LineMode = LineMode.Straight,
            LineSize = 5,
            PointMode = PointMode.Circle,
            PointSize = 10,
            BackgroundColor = SKColors.White
        };

        BarChart3 = new LineChart
        {
            Entries = failedEntries,
            LabelTextSize = 14f,
            LineMode = LineMode.Straight,
            LineSize = 5,
            PointMode = PointMode.Square,
            PointSize = 10,
            BackgroundColor = SKColors.White
        };
    }

    partial void OnShowAggregateDataChanged(bool value)
    {
        _ = LoadChartsAsync();
    }
}