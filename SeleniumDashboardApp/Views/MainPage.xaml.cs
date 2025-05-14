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
    }
}