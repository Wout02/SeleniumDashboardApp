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
    private readonly ApiService _apiService;

    public MainPage(DashboardViewModel viewModel, LocalDatabaseService database, ApiService apiService, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _database = database;
        _apiService = apiService;
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

            var page = new TestRunDetailPage(viewModel, selectedRun.Id);
            await Navigation.PushAsync(page, animated: true);
        }

        TestRunsCollectionView.SelectedItem = null;
    }

    private async void OnDeleteTestRun(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is int id)
        {
            var confirm = await DisplayAlert("Verwijderen", "Weet je zeker dat je deze testrun wilt verwijderen?", "Ja", "Annuleren");
            if (!confirm) return;

            var backendSuccess = await _apiService.DeleteTestRunAsync(id);

            if (backendSuccess)
            {
                await _database.DeleteTestRunByIdAsync(id);

                if (BindingContext is DashboardViewModel viewModel)
                {
                    await viewModel.RefreshTestRuns();
                }

                await DisplayAlert("Verwijderd", "Testrun is verwijderd uit backend en database.", "OK");
            }
            else
            {
                await DisplayAlert("Fout", "Kan testrun niet verwijderen uit backend.", "OK");
            }
        }
    }
}