using CommunityToolkit.Mvvm.ComponentModel;
using SeleniumDashboardApp.Models;
using SeleniumDashboardApp.Services;
using System.Collections.ObjectModel;

namespace SeleniumDashboardApp.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private ObservableCollection<TestRun> testRuns;

        public DashboardViewModel(ApiService apiService)
        {
            _apiService = apiService;
            LoadRuns();
        }

        private async void LoadRuns()
        {
            var runs = await _apiService.GetTestRunsAsync();
            TestRuns = new ObservableCollection<TestRun>(runs);
        }
    }
}