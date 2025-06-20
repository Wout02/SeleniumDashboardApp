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
    private bool _isLoading = false;

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
    }

    [RelayCommand]
    public async Task RefreshTestRuns()
    {
        if (_isLoading) return;

        await LoadAndSyncRunsAsync();
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
        if (_isLoading)
        {
            System.Diagnostics.Debug.WriteLine("[SKIP] LoadAndSyncRunsAsync already running");
            return;
        }

        _isLoading = true;
        try
        {
            System.Diagnostics.Debug.WriteLine("[START] LoadAndSyncRunsAsync");

            var runs = await _apiService.GetTestRunsAsync();

            if (runs != null && runs.Count > 0)
            {
                await _database.DeleteAllAsync();
                System.Diagnostics.Debug.WriteLine("[CLEARED] Local database");

                foreach (var run in runs)
                {
                    var exists = await _apiService.GetTestRunAsync(run.Id);

                    if (exists != null)
                    {
                        var local = new LocalTestRun
                        {
                            BackendId = run.Id,
                            ProjectName = run.ProjectName,
                            Status = run.Status,
                            Date = run.Date,
                            Summary = run.Summary,
                            LogOutput = run.LogOutput
                        };

                        await _database.SaveTestRunAsync(local);
                        System.Diagnostics.Debug.WriteLine($"[SAVED] TestRun {run.Id} to local DB");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[SKIP] Testrun ID {run.Id} niet gevonden in backend.");
                    }
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[EMPTY] No test runs from API");
            }

            await ApplyLocalFiltersAsync();
            System.Diagnostics.Debug.WriteLine("[COMPLETED] LoadAndSyncRunsAsync");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Fout] {ex.Message}");

            await ApplyLocalFiltersAsync();
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task ApplyLocalFiltersAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[START] ApplyLocalFiltersAsync");

            var allRuns = await _database.GetTestRunsAsync();
            System.Diagnostics.Debug.WriteLine($"[LOCAL DB] Found {allRuns.Count} test runs");

            var filtered = allRuns.Where(run =>
                (IsStatusPassedSelected && run.Status == "Passed") ||
                (IsStatusFailedSelected && run.Status == "Failed") ||
                (!IsStatusPassedSelected && !IsStatusFailedSelected)
            ).ToList();

            if (!string.IsNullOrWhiteSpace(SearchProject))
                filtered = filtered.Where(r => r.ProjectName.Contains(SearchProject, StringComparison.OrdinalIgnoreCase)).ToList();

            var converted = filtered.Select(run => new TestRun
            {
                Id = run.BackendId,
                ProjectName = run.ProjectName,
                Status = run.Status,
                Date = run.Date,
                Summary = run.Summary,
                LogOutput = run.LogOutput
            }).ToList();

            var deduped = converted
                .GroupBy(x => x.Id)
                .Select(g => g.First())
                .OrderByDescending(x => x.Date)
                .ToList();

            System.Diagnostics.Debug.WriteLine($"[FILTERED] {deduped.Count} test runs after deduplication");

            TestRuns.Clear();
            foreach (var testRun in deduped)
            {
                TestRuns.Add(testRun);
            }

            System.Diagnostics.Debug.WriteLine("[COMPLETED] ApplyLocalFiltersAsync");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FILTER ERROR] {ex.Message}");
        }
    }

    public async Task AddOrUpdateTestRunAsync(TestRun testRun)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[ADD/UPDATE] TestRun {testRun.Id}");

            var local = new LocalTestRun
            {
                BackendId = testRun.Id,
                ProjectName = testRun.ProjectName,
                Status = testRun.Status,
                Date = testRun.Date,
                Summary = testRun.Summary,
                LogOutput = testRun.LogOutput
            };

            var existing = await _database.GetTestRunByIdAsync(testRun.Id);
            if (existing != null)
            {
                await _database.DeleteTestRunByIdAsync(testRun.Id);
            }

            await _database.SaveTestRunAsync(local);

            await ApplyLocalFiltersAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ADD/UPDATE ERROR] {ex.Message}");
        }
    }
}