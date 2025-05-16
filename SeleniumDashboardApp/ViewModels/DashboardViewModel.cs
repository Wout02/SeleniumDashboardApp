// ViewModel: ViewModels/DashboardViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SeleniumDashboard.Shared;
using SeleniumDashboardApp.Services;
using System.Collections.ObjectModel;
using SeleniumDashboardApp.Models;

namespace SeleniumDashboardApp.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly LocalDatabaseService _database;

        [ObservableProperty]
        private ObservableCollection<TestRun> testRuns;

        [ObservableProperty]
        private string selectedStatus;

        [ObservableProperty]
        private string searchProject;

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

            var localRuns = await _database.GetTestRunsAsync();
            TestRuns = new ObservableCollection<TestRun>(localRuns);
        }

        [RelayCommand]
        public async Task ApplyFilters()
        {
            var url = "api/testrun/filter";
            var query = new List<string>();

            if (!string.IsNullOrWhiteSpace(selectedStatus))
                query.Add($"status={selectedStatus}");

            if (!string.IsNullOrWhiteSpace(searchProject))
                query.Add($"project={searchProject}");

            if (query.Any())
                url += "?" + string.Join("&", query);

            var json = await _apiService.GetRawAsync(url);
            var results = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TestRun>>(json);
            TestRuns = new ObservableCollection<TestRun>(results);
        }

        [RelayCommand]
        public async Task ResetFilters()
        {
            selectedStatus = string.Empty;
            searchProject = string.Empty;
            await LoadAllAsync();
        }

        private async Task LoadAllAsync()
        {
            var runs = await _apiService.GetTestRunsAsync();
            TestRuns = new ObservableCollection<TestRun>(runs);
        }
    }
}

