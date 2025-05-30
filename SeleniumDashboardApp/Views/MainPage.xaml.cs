using SeleniumDashboardApp.ViewModels;
using SeleniumDashboardApp.Services;
using Microsoft.Maui.Controls.Shapes;

namespace SeleniumDashboardApp.Views;

public partial class MainPage : ContentPage
{
    private readonly LocalDatabaseService _database;

    public MainPage(DashboardViewModel viewModel, LocalDatabaseService database)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _database = database;
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
        if (!string.IsNullOrWhiteSpace(status))
        {
            StatusDisplay.Text = $"Status: {status}";
            StatusDisplay.IsVisible = true;
        }
        else
        {
            StatusDisplay.Text = string.Empty;
            StatusDisplay.IsVisible = false;
        }
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
        if (e.CurrentSelection.FirstOrDefault() is Models.LocalTestRun selectedRun)
        {
            var viewModel = new TestRunDetailsViewModel(_database);
            await viewModel.LoadTestRunById(selectedRun.Id);

            var tabPage = new TestRunDetailsTabbedPage(viewModel);
            await Navigation.PushAsync(tabPage, animated: true);

            ((CollectionView)sender).SelectedItem = null;
        }
    }
}