using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;
using SeleniumDashboardApp.Models;
using SeleniumDashboardApp.Services;

namespace SeleniumDashboardApp.ViewModels;

public class TestRunDetailViewModel : ObservableObject
{
    private readonly LocalDatabaseService _database;

    public TestRunDetailViewModel(LocalDatabaseService database)
    {
        _database = database;
        ChangeTabCommand = new Command<string>(tab => SelectedTab = tab);
    }

    private string _selectedTab = "Details";
    public string SelectedTab
    {
        get => _selectedTab;
        set => SetProperty(ref _selectedTab, value);
    }

    private LocalTestRun? _selectedTestRun;
    public LocalTestRun? SelectedTestRun
    {
        get => _selectedTestRun;
        set => SetProperty(ref _selectedTestRun, value);
    }

    public ICommand ChangeTabCommand { get; }

    public async Task LoadTestRunById(int id)
    {
        var testRun = await _database.GetTestRunByIdAsync(id);
        if (testRun != null)
        {
            SelectedTestRun = testRun;
        }
    }
}