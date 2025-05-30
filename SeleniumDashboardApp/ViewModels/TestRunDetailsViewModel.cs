using CommunityToolkit.Mvvm.ComponentModel;
using SeleniumDashboard.Shared;
using SeleniumDashboardApp.Models;
using SeleniumDashboardApp.Services;

namespace SeleniumDashboardApp.ViewModels;

public partial class TestRunDetailsViewModel : ObservableObject, IQueryAttributable
{
    private readonly LocalDatabaseService _database;

    public TestRunDetailsViewModel(LocalDatabaseService database)
    {
        _database = database;
    }

    [ObservableProperty]
    private LocalTestRun selectedTestRun;

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var idValue) && idValue is string idString && int.TryParse(idString, out int id))
        {
            await LoadTestRunById(id);
        }
    }

    public async Task LoadTestRunById(int id)
    {
        var allRuns = await _database.GetTestRunsAsync();
        SelectedTestRun = allRuns.FirstOrDefault(r => r.Id == id);
    }
}
