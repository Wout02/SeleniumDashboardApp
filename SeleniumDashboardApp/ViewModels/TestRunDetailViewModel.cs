using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;
using SeleniumDashboard.Shared;
using SeleniumDashboardApp.Services;

namespace SeleniumDashboardApp.ViewModels;

public partial class TestRunDetailViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    private TestRun? selectedTestRun;

    private string _selectedTab = "Details";
    public string SelectedTab
    {
        get => _selectedTab;
        set => SetProperty(ref _selectedTab, value);
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
}