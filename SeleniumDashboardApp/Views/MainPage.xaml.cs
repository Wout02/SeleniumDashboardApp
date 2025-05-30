using SeleniumDashboardApp.Models;
using SeleniumDashboardApp.Services;
using SeleniumDashboardApp.ViewModels;

namespace SeleniumDashboardApp.Views
{
    public partial class MainPage : ContentPage
    {
        private readonly LocalDatabaseService _database;

        public MainPage(DashboardViewModel viewModel, LocalDatabaseService database)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _database = database;
        }

        private void OnToggleSearchClicked(object sender, EventArgs e)
        {
            SearchEntry.IsVisible = !SearchEntry.IsVisible;
        }

        private void OnToggleFilterClicked(object sender, EventArgs e)
        {
            StatusPicker.IsVisible = !StatusPicker.IsVisible;
            FilterButtons.IsVisible = !FilterButtons.IsVisible;
        }

        private async void OnTestRunTapped(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is LocalTestRun selectedRun)
            {
                var viewModel = new TestRunDetailsViewModel(_database);
                await viewModel.LoadTestRunById(selectedRun.Id);

                var tabPage = new TestRunDetailsTabbedPage(viewModel);
                await Navigation.PushAsync(tabPage);

                ((CollectionView)sender).SelectedItem = null; // deselecteer
            }
        }
    }
}