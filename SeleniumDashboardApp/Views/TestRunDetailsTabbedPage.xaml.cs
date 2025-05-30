using SeleniumDashboardApp.ViewModels;

namespace SeleniumDashboardApp.Views;

public partial class TestRunDetailsTabbedPage : TabbedPage
{
    public TestRunDetailsTabbedPage(TestRunDetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
