using SeleniumDashboardApp.ViewModels;

namespace SeleniumDashboardApp.Views
{
    public partial class MainPage : ContentPage
    {
        public MainPage(DashboardViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
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
    }
}