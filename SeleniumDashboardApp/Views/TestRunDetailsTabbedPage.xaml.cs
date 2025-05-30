using SeleniumDashboardApp.ViewModels;

namespace SeleniumDashboardApp.Views;

public partial class TestRunDetailsTabbedPage : TabbedPage
{
    public TestRunDetailsTabbedPage(TestRunDetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        BackgroundColor = Colors.White;
        BarBackgroundColor = Color.FromArgb("#1E1E2E"); // donkere topbar
        BarTextColor = Colors.White;
    }
}
