using CommunityToolkit.Mvvm.ComponentModel;
using SeleniumDashboard.Shared;
using SeleniumDashboardApp.Services;
using System.Collections.ObjectModel;
using SeleniumDashboardApp.Models;

namespace SeleniumDashboardApp.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private ObservableCollection<TestRun> testRuns;

        private readonly LocalDatabaseService _database;

        public DashboardViewModel(ApiService apiService, LocalDatabaseService database)
        {
            _apiService = apiService;
            _database = database;
            LoadRuns();
        }

        private async void LoadRuns()
        {
            var runs = await _apiService.GetTestRunsAsync();

            // Opslaan in lokale database
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

            // Laden uit database
            var localRuns = await _database.GetTestRunsAsync();
            TestRuns = new ObservableCollection<TestRun>(localRuns);
        }
    }
}