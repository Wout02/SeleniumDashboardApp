using SeleniumDashboardApp.ViewModels;

namespace SeleniumDashboardApp.Views;

public partial class TestRunGraphPage : ContentPage
{
    public TestRunGraphPage() : this(null) { }

    public TestRunGraphPage(TestRunDetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
