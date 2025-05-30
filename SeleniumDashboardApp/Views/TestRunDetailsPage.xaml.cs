using SeleniumDashboardApp.ViewModels;

namespace SeleniumDashboardApp.Views;

public partial class TestRunDetailsPage : ContentPage
{
    public TestRunDetailsPage() : this(null) { }

    public TestRunDetailsPage(TestRunDetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}