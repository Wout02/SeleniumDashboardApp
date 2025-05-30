using SeleniumDashboardApp.ViewModels;

namespace SeleniumDashboardApp.Views;

public partial class TestRunLogsPage : ContentPage
{
    public TestRunLogsPage() : this(null) { }

    public TestRunLogsPage(TestRunDetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}

