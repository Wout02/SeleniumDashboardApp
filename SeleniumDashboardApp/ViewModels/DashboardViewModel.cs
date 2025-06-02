using CommunityToolkit.Mvvm.ComponentModel;
using SeleniumDashboard.Shared;
using SeleniumDashboardApp.Services;
using SeleniumDashboardApp.Models;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;

namespace SeleniumDashboardApp.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly ApiService _apiService;
    private readonly LocalDatabaseService _database;

    [ObservableProperty]
    private ObservableCollection<TestRun> testRuns = new();

    [ObservableProperty]
    private string searchProject;

    [ObservableProperty]
    private bool isStatusPassedSelected;

    [ObservableProperty]
    private bool isStatusFailedSelected;

    public DashboardViewModel(ApiService apiService, LocalDatabaseService database)
    {
        _apiService = apiService;
        _database = database;
        _ = LoadAndSyncRunsAsync();
    }

    [RelayCommand]
    private void ResetFilters()
    {
        IsStatusPassedSelected = false;
        IsStatusFailedSelected = false;
        SearchProject = string.Empty;
    }

    public string SelectedStatus =>
        IsStatusPassedSelected ? "Passed" :
        IsStatusFailedSelected ? "Failed" :
        string.Empty;

    partial void OnSearchProjectChanged(string value)
    {
        _ = ApplyLocalFiltersAsync();
    }

    partial void OnIsStatusPassedSelectedChanged(bool value)
    {
        OnPropertyChanged(nameof(SelectedStatus));
        _ = ApplyLocalFiltersAsync();
    }

    partial void OnIsStatusFailedSelectedChanged(bool value)
    {
        OnPropertyChanged(nameof(SelectedStatus));
        _ = ApplyLocalFiltersAsync();
    }

    private async Task LoadAndSyncRunsAsync()
    {
        try
        {
            var runs = await _apiService.GetTestRunsAsync();

            if (runs != null && runs.Count > 0)
            {
                await _database.DeleteAllAsync();
                foreach (var run in runs)
                {
                    var local = new LocalTestRun
                    {
                        Id = run.Id,
                        ProjectName = run.ProjectName,
                        Status = run.Status,
                        Date = run.Date,
                        Summary = run.Summary
                    };
                    await _database.SaveTestRunAsync(local);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Fout] {ex.Message}");
        }

        await ApplyLocalFiltersAsync();
    }

    private async Task ApplyLocalFiltersAsync()
    {
        var allRuns = await _database.GetTestRunsAsync();

        var filtered = allRuns.Where(run =>
            (IsStatusPassedSelected && run.Status == "Passed") ||
            (IsStatusFailedSelected && run.Status == "Failed") ||
            (!IsStatusPassedSelected && !IsStatusFailedSelected)
        ).ToList();

        if (!string.IsNullOrWhiteSpace(SearchProject))
            filtered = filtered.Where(r => r.ProjectName.Contains(SearchProject, StringComparison.OrdinalIgnoreCase)).ToList();

        var converted = filtered.Select(run => new TestRun
        {
            Id = run.Id,
            ProjectName = run.ProjectName,
            Status = run.Status,
            Date = run.Date,
            Summary = run.Summary
        }).ToList();

        TestRuns = new ObservableCollection<TestRun>(converted);
    }
}