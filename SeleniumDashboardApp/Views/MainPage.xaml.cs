using SeleniumDashboardApp.ViewModels;
using SeleniumDashboardApp.Services;
using SeleniumDashboardApp.Views;
using Microsoft.Maui.Controls.Shapes;
using SeleniumDashboard.Shared;

namespace SeleniumDashboardApp.Views;

public partial class MainPage : ContentPage
{
    private readonly LocalDatabaseService _database;
    private readonly IServiceProvider _serviceProvider;

    public MainPage(DashboardViewModel viewModel, LocalDatabaseService database, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _database = database;
        _serviceProvider = serviceProvider;
        BackgroundColor = Colors.White;
        Title = "Selenium Dashboard";

        viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(viewModel.SelectedStatus))
                UpdateStatusDisplay(viewModel.SelectedStatus);
        };
    }

    private void UpdateStatusDisplay(string status)
    {
        StatusDisplay.Text = string.IsNullOrWhiteSpace(status) ? string.Empty : $"Status: {status}";
        StatusDisplay.IsVisible = !string.IsNullOrWhiteSpace(status);
    }

    private void OnToggleSearchClicked(object sender, EventArgs e)
    {
        SearchEntryFrame.IsVisible = !SearchEntryFrame.IsVisible;
    }

    private void OnToggleFilterClicked(object sender, EventArgs e)
    {
        StatusPickerFrame.IsVisible = !StatusPickerFrame.IsVisible;
    }

    private async void OnTestRunTapped(object sender, SelectionChangedEventArgs e)
    {
        Console.WriteLine("TestRun geklikt");

        if (e.CurrentSelection.FirstOrDefault() is TestRun selectedRun)
        {
            Console.WriteLine($"Geselecteerde run: {selectedRun.Id}");

            var viewModel = _serviceProvider.GetService<TestRunDetailViewModel>();
            await viewModel.LoadTestRunById(selectedRun.Id);

            var page = new TestRunDetailPage(viewModel);
            await Navigation.PushAsync(page, animated: true);
        }

        TestRunsCollectionView.SelectedItem = null;
    }
}